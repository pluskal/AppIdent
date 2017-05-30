//Copyright (c) 2017 Jan Pluskal
//
//Permission is hereby granted, free of charge, to any person
//obtaining a copy of this software and associated documentation
//files (the "Software"), to deal in the Software without
//restriction, including without limitation the rights to use,
//copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the
//Software is furnished to do so, subject to the following
//conditions:
//
//The above copyright notice and this permission notice shall be
//included in all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
//OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
//HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
//WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
//FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
//OTHER DEALINGS IN THE SOFTWARE.



using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using AppIdent.Models;
using Framework.Models;

namespace AppIdent.Statistics
{
    [DataContract]
    public class ApplicationProtocolClassificationStatisticsMeter
    {
        [DataMember]
        public ConcurrentDictionary<string, ApplicationProtocolClassificationStatistics> AppStatistics { get; set; } =
            new ConcurrentDictionary<string, ApplicationProtocolClassificationStatistics>(StringComparer.OrdinalIgnoreCase);
        
        public ApplicationProtocolClassificationStatistics this[string appTag]
        {
            get
            {
                ApplicationProtocolClassificationStatistics stats;
                this.AppStatistics.TryGetValue(appTag, out stats);
                return stats;
            }
        }

        public void PrintResults() { Console.WriteLine(this.ToString()); }

        #region Overrides of Object
        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach(var applicationProtocolPrecision in this.AppStatistics.OrderBy(KeyValuePair => KeyValuePair.Key).Select(kvp => kvp.Value)
                .OrderByDescending(appStat => appStat.TP)) { sb.AppendLine(applicationProtocolPrecision.ToString()); }
            return sb.ToString();
        }
        #endregion

        public void UpdateStatistics(string predictedAppTag, string appTag)
        {
            predictedAppTag = predictedAppTag?.ToLower();
            appTag = appTag?.ToLower();
            if(appTag == null || predictedAppTag == null) { return; }

            if(!this.AppStatistics.ContainsKey(predictedAppTag)) { this.AppStatistics.TryAdd(predictedAppTag, new ApplicationProtocolClassificationStatistics(predictedAppTag)); }

            if(!this.AppStatistics.ContainsKey(appTag)) { this.AppStatistics.TryAdd(appTag, new ApplicationProtocolClassificationStatistics(appTag)); }

            if(appTag == predictedAppTag) { this.AppStatistics[predictedAppTag].AddTP(predictedAppTag); }
            else if(appTag != predictedAppTag)
            {
                this.AppStatistics[predictedAppTag].AddFP(appTag);
                this.AppStatistics[appTag].AddFN(predictedAppTag);
            }
        }

        public void SaveToCsv(string csvFilePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("sep=;");
            sb.AppendLine($"PredictedAppTag;TP;FP;FN;Precission;Recall;FMeasure;");
            foreach (var appStat in this.AppStatistics.Values)
            {
                sb.AppendLine($"{appStat.PredictedAppTag.Replace(",", " ")};{appStat.TP};{appStat.FP};{appStat.FN};{appStat.Precission};{appStat.Recall};{appStat.FMeasure};");
            }
            var csv = sb.ToString();
            using(Stream myStream = new FileStream(csvFilePath,FileMode.Create))
            using(var sw = new StreamWriter(myStream, Encoding.UTF8))
            {
                sw.Write(csv);
                sw.Flush(); //otherwise you are risking empty stream
                myStream.Seek(0, SeekOrigin.Begin);
            }

        }
    }
}
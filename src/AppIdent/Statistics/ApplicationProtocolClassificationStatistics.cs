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



using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using AppIdent.EPI;
using AppIdent.Models;

namespace AppIdent.Statistics
{
    [DataContract]
    public class ApplicationProtocolClassificationStatistics
    {
        [DataMember] public Dictionary<string, int> FNsStatistics = new Dictionary<string, int>();

        [DataMember] public Dictionary<string, int> FPsStatistics = new Dictionary<string, int>();

        public ApplicationProtocolClassificationStatistics(string predictedAppTag) { this.PredictedAppTag = predictedAppTag; }

        public ApplicationProtocolClassificationStatistics(ProtocolModelCluster protocolModelCluster) { this.PredictedAppTag = protocolModelCluster.ClusterAppTags; }

        [DataMember]
        public string PredictedAppTag { get; set; }

        [DataMember]
        public int TP { get; set; }

        [DataMember]
        public int FP { get; set; }

        [DataMember]
        public int FN { get; set; }

        public double Precission => Utilities.Precission(this.TP, this.FP);
        public double Recall => Utilities.Recall(this.TP, this.TP + this.FN);
        public double FMeasure => Utilities.F_Measure(this.TP, this.FP, this.TP + this.FN);
        

        public string StatisticSummary
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine($"TP:{this.TP}, FP:{this.FP}, FN:{this.FN}");
                sb.Append($"FPs: ");
                foreach(var fpStat in this.FPsStatistics) { sb.Append($"{fpStat.Key}: {fpStat.Value}, "); }
                sb.Append($"\nFNs: ");
                foreach(var fnStat in this.FNsStatistics) { sb.Append($"{fnStat.Key}: {fnStat.Value}, "); }
                sb.AppendLine($"\nPrecision: {this.Precission}");
                sb.AppendLine($"Recall: {this.Recall}");
                sb.AppendLine($"F-Measure: {this.FMeasure}");
                return sb.ToString();
            }
        }

        public void AddFN(string predictedAppTag)
        {
            this.FN++;
            if(this.FNsStatistics.ContainsKey(predictedAppTag)) { this.FNsStatistics[predictedAppTag]++; }
            else { this.FNsStatistics.Add(predictedAppTag, 1); }
        }

        public void AddFP(string predictedAppTag)
        {
            this.FP++;
            if(this.FPsStatistics.ContainsKey(predictedAppTag)) { this.FPsStatistics[predictedAppTag]++; }
            else { this.FPsStatistics.Add(predictedAppTag, 1); }
        }

        public void AddTP(string predictedAppTag) { this.TP++; }

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
            sb.AppendLine($"{this.PredictedAppTag}");
            sb.AppendLine(this.StatisticSummary);
            return sb.ToString();
        }
        #endregion
    }
}
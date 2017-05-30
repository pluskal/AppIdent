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
using System.Collections.Generic;
using System.Linq;
using AppIdent.Metrics;
using AppIdent.Misc;
using AppIdent.Models;
using Framework.Models;
using Netfox.Core.Enums;

namespace AppIdent.Features.Bases
{
    public class ByteFrequencyBase : FeatureBase
    {
        public ByteFrequencyBase() { }
        public ByteFrequencyBase(L7Conversation l7Conversation, DaRFlowDirection flowDirection) : base(l7Conversation, flowDirection) { }
        public ByteFrequencyBase(double featureValue) : base(featureValue) { }

        public override FeatureKind FeatureKind { get; } = FeatureKind.Continous;

        public override double ComputeDistanceToProtocolModel(FeatureBase sampleFeature)
        {
            return Math.Abs(this.Normalize(this.FeatureValue) - this.Normalize(sampleFeature.FeatureValue));
        }

        public override double ComputeFeature(L7Conversation l7Conversation, DaRFlowDirection flowDirection)
        {
            var byteFreq = new double[256];
            byte[] firstMsg = null;
            switch(flowDirection)
            {
                case DaRFlowDirection.up:
                    firstMsg = l7Conversation.UpFlowPDUs.FirstOrDefault()?.PDUByteArr;
                    break;
                case DaRFlowDirection.down:
                    firstMsg = l7Conversation.DownFlowPDUs.FirstOrDefault()?.PDUByteArr;
                    break;
                case DaRFlowDirection.non:
                    firstMsg = l7Conversation.L7PDUs.FirstOrDefault()?.PDUByteArr;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(flowDirection), flowDirection, null);
            }

            if(firstMsg == null) { return -1; }

            for(var i = 0; i < firstMsg.Length; i++) { byteFreq[Convert.ToInt32(firstMsg.GetValue(i))]++; }
            var tmpMin = 1.0;
            var freq = new List<double>();
            for(var i = 0; i < 256; i++)
            {
                if(!byteFreq[i].Equals(0.0))
                {
                    byteFreq[i] /= firstMsg.Length;
                    if(tmpMin.CompareTo(byteFreq[i]) > 0) { tmpMin = byteFreq[i]; }
                    freq.Add(byteFreq[i]);
                }
                //Console.WriteLine("Byte: " + i + " FeatureValue: " + byteFreq[i] );
            }
            return Utilities.StandardDeviation(freq);
        }

        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            this.FeatureValue = FeatureMetrics.FeatureMetricAverage(featureValues);
            this.Weight = WeightMetrics.WeightUsingNormEntropy(featureValues);
        }
    }
}
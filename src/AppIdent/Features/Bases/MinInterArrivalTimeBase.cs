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
    public class MinInterArrivalTimeBase : FeatureBase
    {
        public MinInterArrivalTimeBase() { }
        public MinInterArrivalTimeBase(L7Conversation l7Conversation, DaRFlowDirection flowDirection) : base(l7Conversation, flowDirection) { }
        public MinInterArrivalTimeBase(double featureValue) : base(featureValue) { }

        public override FeatureKind FeatureKind { get; } = FeatureKind.Continous;
        public override double WeightBias { get; } = 0.3;

        public override double ComputeDistanceToProtocolModel(FeatureBase sampleFeature)
        {
            return Math.Abs(this.Normalize(this.FeatureValue) - this.Normalize(sampleFeature.FeatureValue));
        }

        public override double ComputeFeature(L7Conversation l7Conversation, DaRFlowDirection flowDirection)
        {
            IEnumerable<L7PDU> pdus;
            switch(flowDirection)
            {
                case DaRFlowDirection.up:
                    pdus = l7Conversation.UpFlowPDUs;
                    break;
                case DaRFlowDirection.down:
                    pdus = l7Conversation.DownFlowPDUs;
                    break;
                case DaRFlowDirection.non:
                    pdus = l7Conversation.L7PDUs;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(flowDirection), flowDirection, null);
            }

            var l7Pdus = pdus as L7PDU[] ?? pdus.ToArray();
            if(!l7Pdus.Any()) { return -1; }

            var length = l7Pdus.Count();
            var pdusArray = l7Pdus.ToArray();

            if(length <= 1) { return 0; }

            var minTime = Math.Abs((pdusArray[1].FirstSeen - pdusArray[0].FirstSeen).TotalSeconds);

            for(var i = 1; i < length - 1; i++)
            {
                var interArrivalTime = Math.Abs((pdusArray[i + 1].FirstSeen - pdusArray[i].FirstSeen).TotalSeconds);

                if(minTime.CompareTo(interArrivalTime) > 0) { minTime = interArrivalTime; }
            }

            return minTime;
        }

        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            this.FeatureValue = FeatureMetrics.FeatureMetricAverage(featureValues);
            this.Weight = WeightMetrics.WeightUsingNormEntropy(featureValues);
        }
    }
}
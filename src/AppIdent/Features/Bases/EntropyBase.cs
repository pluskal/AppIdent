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
    public class EntropyBase : FeatureBase
    {
        public EntropyBase() { }
        public EntropyBase(L7Conversation l7Conversation, DaRFlowDirection flowDirection) : base(l7Conversation, flowDirection) { }
        public EntropyBase(double featureValue) : base(featureValue) { }

        public override FeatureKind FeatureKind { get; } = FeatureKind.Continous;

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
                default: throw new ArgumentOutOfRangeException(nameof(flowDirection), flowDirection, null);
            }
            var l7Pdus = pdus.Take(10).ToArray();
            var histogram = l7Pdus.SelectMany(pdu => pdu.PDUByteArr).GroupBy(byteValue => byteValue).Select(g => new Entropy.EntropyItem<byte>
            {
                Key = g.Key,
                Count = g.Count()
            });

            return Entropy.Calculate(histogram);
        }

        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            this.FeatureValue = FeatureMetrics.FeatureMetricAverage(featureValues);
            this.Weight = WeightMetrics.WeightUsingStandarDeviation(featureValues);
        }
    }
}
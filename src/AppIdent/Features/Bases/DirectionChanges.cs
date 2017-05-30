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
using System.Linq;
using AppIdent.Metrics;
using AppIdent.Misc;
using AppIdent.Models;
using Framework.Models;
using Netfox.Core.Enums;

namespace AppIdent.Features.Bases
{
    public class DirectionChanges : FeatureBase
    {
        public DirectionChanges() { }

        public DirectionChanges(L7Conversation l7Conversation) : base(l7Conversation, DaRFlowDirection.non) { }

        // This constructor is also needed (called from FeatureBaseTests)
        public DirectionChanges(L7Conversation l7Conversation, DaRFlowDirection flowDirection) : base(l7Conversation, DaRFlowDirection.non) { }

        public DirectionChanges(double featureValue) : base(featureValue) { }

        public override FeatureKind FeatureKind { get; } = FeatureKind.Continous;

        public override double ComputeDistanceToProtocolModel(FeatureBase sampleFeature)
        {
            return Math.Abs(this.Normalize(this.FeatureValue) - this.Normalize(sampleFeature.FeatureValue));
        }

        public override double ComputeFeature(L7Conversation l7Conversation, DaRFlowDirection flowDirection)
        {
            var communicationDirectionChanges = 0;
            var l7Pdus = l7Conversation.L7PDUs as L7PDU[] ?? l7Conversation.L7PDUs?.ToArray();
            if(l7Pdus == null) { return -1; }

            var srcIp = l7Pdus.FirstOrDefault()?.SourceEndPoint;

            foreach(var pdu in l7Pdus)
            {
                if(srcIp.Equals(pdu.SourceEndPoint)) { continue; }

                communicationDirectionChanges++;
                srcIp = pdu.SourceEndPoint;
            }

            return communicationDirectionChanges;
        }

        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            this.FeatureValue = FeatureMetrics.FeatureMetricAverage(featureValues);
            this.Weight = WeightMetrics.WeightUsingNormEntropy(featureValues);
        }
    }
}
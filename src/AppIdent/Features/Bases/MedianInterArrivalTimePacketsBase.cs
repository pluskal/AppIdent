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
using Framework.Models.PmLib.Frames;
using Netfox.Core.Enums;

namespace AppIdent.Features.Bases
{
    public class MedianInterArrivalTimePacketsBase : FeatureBase
    {
        public MedianInterArrivalTimePacketsBase() { }
        public MedianInterArrivalTimePacketsBase(L7Conversation l7Conversation, DaRFlowDirection flowDirection) : base(l7Conversation, flowDirection) { }
        public MedianInterArrivalTimePacketsBase(double featureValue) : base(featureValue) { }
        public override FeatureKind FeatureKind { get; } = FeatureKind.Continous;
        public override double WeightBias { get; } = 0.3;

        public override double ComputeDistanceToProtocolModel(FeatureBase sampleFeature)
        {
            return Math.Abs(this.Normalize(this.FeatureValue) - this.Normalize(sampleFeature.FeatureValue));
        }

        public override double ComputeFeature(L7Conversation l7Conversation, DaRFlowDirection flowDirection)
        {
            IEnumerable<PmFrameBase> frames;
            switch(flowDirection)
            {
                case DaRFlowDirection.up:
                    frames = l7Conversation.UpFlowFrames.OrderBy(i => i.FirstSeen);
                    break;
                case DaRFlowDirection.down:
                    frames = l7Conversation.DownFlowFrames.OrderBy(i => i.FirstSeen);
                    break;
                case DaRFlowDirection.non:
                    frames = l7Conversation.Frames.OrderBy(i => i.FirstSeen);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(flowDirection), flowDirection, null);
            }

            var pmFrameBases = frames as PmFrameBase[] ?? frames.ToArray();
            if(pmFrameBases == null || !pmFrameBases.Any()) { return -1; }

            var length = pmFrameBases.Count();
            if(length <= 1) { return 0; }

            var halfIndex = length / 2;
            var median = 0.0;
            var times = new double[length];

            for(var i = 0; i < length - 1; i++) { times[i] = (pmFrameBases[i + 1].FirstSeen - pmFrameBases[i].FirstSeen).TotalSeconds; }

            var sortedTimes = times.OrderBy(n => n);
            if((length % 2) == 0) { median = (sortedTimes.ElementAt(halfIndex) + sortedTimes.ElementAt(halfIndex - 1)) / 2; }
            else { median = sortedTimes.ElementAt(halfIndex); }
            return median;
        }

        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            this.FeatureValue = FeatureMetrics.FeatureMetricAverage(featureValues);
            this.Weight = WeightMetrics.WeightUsingNormEntropy(featureValues);
        }
    }
}
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
    public class ThirdQuartileControlBytesBase : FeatureBase
    {
        public ThirdQuartileControlBytesBase() { }
        public ThirdQuartileControlBytesBase(L7Conversation l7Conversation, DaRFlowDirection flowDirection) : base(l7Conversation, flowDirection) { }
        public ThirdQuartileControlBytesBase(double featureValue) : base(featureValue) { }

        public override FeatureKind FeatureKind { get; } = FeatureKind.Continous;

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
                    frames = l7Conversation.UpFlowFrames;
                    break;
                case DaRFlowDirection.down:
                    frames = l7Conversation.DownFlowFrames;
                    break;
                case DaRFlowDirection.non:
                    frames = l7Conversation.Frames;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(flowDirection), flowDirection, null);
            }

            var Frames = frames as PmFrameBase[] ?? frames.ToArray();
            if(!Frames.Any()) { return -1; }

            var length = Frames.Count();
            if(length <= 1) { return 0; }
            var bytes = new double[length];

            for(var i = 0; i < length; i++)
            {
                if(Frames[i].L7Offset == -1) { bytes[i] = Frames[i].OriginalLengthWithoutPadding - (Frames[i].L4Offset - Frames[i].L2Offset); }
                else
                {
                    var dataSize = (int) (Frames[i].OriginalLengthWithoutPadding - (Frames[i].L7Offset - Frames[i].L2Offset));
                    bytes[i] = Frames[i].OriginalLengthWithoutPadding - (Frames[i].L4Offset - Frames[i].L2Offset) - dataSize;
                }
            }

            int medianIndex;
            if(bytes.Length == 1) { medianIndex = 1; }
            else { medianIndex = bytes.Length / 2; }
            Array.Sort(bytes);
            return GetMedian(bytes.Reverse().Take(medianIndex).ToArray());
        }

        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            this.FeatureValue = FeatureMetrics.FeatureMetricAverage(featureValues);
            this.Weight = WeightMetrics.WeightUsingStandarDeviation(featureValues);
            if(this.FeatureValue > 24) { this.Weight = 1; }
            if(this.FeatureValue == 20) { this.Weight = 0; }
        }

        public static double GetMedian(double[] source)
        {
            var temp = source;
            Array.Sort(temp);

            var count = temp.Length;
            if(count == 0) { throw new InvalidOperationException("Empty collection"); }
            if(count % 2 == 0)
            {
                var a = temp[count / 2 - 1];
                var b = temp[count / 2];
                return (a + b) / 2.0;
            }
            return temp[count / 2];
        }
    }
}
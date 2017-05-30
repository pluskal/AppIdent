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
    public class MinControlBytesBase : FeatureBase
    {
        public MinControlBytesBase() { }
        public MinControlBytesBase(L7Conversation l7Conversation, DaRFlowDirection flowDirection) : base(l7Conversation, flowDirection) { }
        public MinControlBytesBase(double featureValue) : base(featureValue) { }

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

            int controlBytes;
            if(Frames.First().L7Offset == -1) { controlBytes = (int) (Frames.First().OriginalLengthWithoutPadding - (Frames.First().L4Offset - Frames.First().L2Offset)); }
            else
            {
                var dataSize = (int) (Frames.First().OriginalLengthWithoutPadding - (Frames.First().L7Offset - Frames.First().L2Offset));
                controlBytes = (int) (Frames.First().OriginalLengthWithoutPadding - (Frames.First().L4Offset - Frames.First().L2Offset) - dataSize);
            }
            foreach(var frame in Frames)
            {
                int tmpControlBytes;
                if(frame.L7Offset == -1) { tmpControlBytes = (int) (frame.OriginalLengthWithoutPadding - (frame.L4Offset - frame.L2Offset)); }
                else
                {
                    var dataSize = (int) (frame.OriginalLengthWithoutPadding - (frame.L7Offset - frame.L2Offset));
                    tmpControlBytes = (int) (frame.OriginalLengthWithoutPadding - (frame.L4Offset - frame.L2Offset) - dataSize);
                }
                if(controlBytes.CompareTo(tmpControlBytes) > 0) { controlBytes = tmpControlBytes; }
            }
            return controlBytes;
        }

        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            this.FeatureValue = FeatureMetrics.FeatureMetricAverage(featureValues);
            this.Weight = WeightMetrics.WeightUsingStandarDeviation(featureValues);
            if(this.FeatureValue < 20 || this.FeatureValue > 24) { this.Weight = 1; }
            if(this.FeatureValue == 20) { this.Weight = 0; }
        }
    }
}
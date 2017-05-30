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
using System.Runtime.Serialization;
using AppIdent.Misc;
using AppIdent.Models;
using Framework.Models;
using Netfox.Core.Enums;

namespace AppIdent.Features.Bases
{
    [DataContract]
    public class First4BytesHashBase : FeatureBase
    {
        public First4BytesHashBase(double featureValue) : base(featureValue) { }
        public First4BytesHashBase() { }
        public First4BytesHashBase(L7Conversation l7Conversation, DaRFlowDirection flowDirection) : base(l7Conversation, flowDirection) { }

        public override FeatureKind FeatureKind { get; } = FeatureKind.Discrete;
        public override FeatureValueRange FeatureValueRange { get; } = new FeatureValueRange(-1, 256);
        [DataMember]
        private byte[] FeatureHashValue { get; set; }

        [DataMember]
        private byte[][] ModelHashValues { get; set; }

        public override double ComputeDistanceToProtocolModel(FeatureBase sampleFeature)
        {
            var featureValue = sampleFeature as First4BytesHashBase;
            if(this.ModelHashValues != null)
            {
                foreach(var modelHashValue in this.ModelHashValues) { if(modelHashValue.SequenceEqual(featureValue.FeatureHashValue)) { return 0; } }
            }
            else { if(this.FeatureHashValue.SequenceEqual(featureValue.FeatureHashValue)) { return 0; } }

            return 1;
        }

        public override double ComputeFeature(L7Conversation l7Conversation, DaRFlowDirection flowDirection)
        {
            byte[] firstMsg = null;
            switch(flowDirection)
            {
                case DaRFlowDirection.up:
                    firstMsg = l7Conversation.UpFlowPDUs.FirstOrDefault()?.PDUByteArr;
                    break;
                case DaRFlowDirection.down:
                    firstMsg = l7Conversation.DownFlowPDUs.FirstOrDefault()?.PDUByteArr;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(flowDirection), flowDirection, null);
            }

            if(firstMsg == null)
            {
                this.FeatureHashValue = new byte[1]
                {
                    0
                };
                return -1;
            }

            this.FeatureHashValue = firstMsg.Take(4).ToArray();

            var hash = 0.0;

            foreach(var b in this.FeatureHashValue) { hash += b; }

            return hash / 4;
        }

        /// <summary>
        ///     http://math.stackexchange.com/questions/395121/how-entropy-scales-with-sample-size
        /// </summary>
        /// <param name="featureValues"></param>
        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            var featuresTyped = featureValues as IFeatureCollectionWrapper<First4BytesHashBase>;
            var hashAndCounts = featuresTyped.GroupBy(feature => feature.FeatureHashValue, new ArrayEqualityComparer<byte>()).Select(g => new Entropy.EntropyItem<byte[]>
            {
                Key = g.Key,
                Count = g.Count()
            }).ToArray();

            this.ModelHashValues = hashAndCounts.Select(hashAndCount => hashAndCount.Key).ToArray();

            var normEnth = 0.0;
            var n = hashAndCounts.Count();

            if(n != 1)
            {
                var enth = Entropy.Calculate(hashAndCounts);
                normEnth = enth / Math.Log(n);
            }
            this.Weight = 1 - normEnth;
        }
    }
}
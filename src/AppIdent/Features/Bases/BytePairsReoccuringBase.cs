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
    public class BytePairsReoccuringBase : FeatureBase
    {
        public BytePairsReoccuringBase() { }
        public BytePairsReoccuringBase(L7Conversation l7Conversation, DaRFlowDirection flowDirection) : base(l7Conversation, flowDirection) { }
        public BytePairsReoccuringBase(double featureValue) : base(featureValue) { }

        public override FeatureKind FeatureKind { get; } = FeatureKind.Continous;

        [DataMember]
        private byte[] FeatureBytePairsValue { get; set; }

        [DataMember]
        private byte[][] ModelBytePairsValues { get; set; }

        public override double ComputeDistanceToProtocolModel(FeatureBase sampleFeature)
        {
            var featureValue = sampleFeature as BytePairsReoccuringBase;
            if(this.ModelBytePairsValues != null)
            {
                foreach(var modelBytePairsValue in this.ModelBytePairsValues) { if(modelBytePairsValue.SequenceEqual(featureValue.FeatureBytePairsValue)) { return 0; } }
            }
            else { if(this.FeatureBytePairsValue.SequenceEqual(featureValue.FeatureBytePairsValue)) { return 0; } }
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
                this.FeatureBytePairsValue = new byte[1]
                {
                    0
                };
                return -1;
            }

            var arrLength = firstMsg.GetLength(0);
            if(arrLength < 2)
            {
                this.FeatureBytePairsValue = new byte[1]
                {
                    0
                };
                return -1;
            }
            var byteReoccuring = 0;

            if(arrLength > 16) { arrLength = 16; }

            var returnArr = new byte[arrLength];
            if(firstMsg[0] == firstMsg[1]) { returnArr[0] = 1; }
            else { returnArr[0] = 0; }
            for(var i = 1; i < arrLength; i++)
            {
                if(firstMsg[i] == firstMsg[i - 1])
                {
                    byteReoccuring++;
                    returnArr[i] = 1;
                }
                else { returnArr[i] = 0; }
            }

            this.FeatureBytePairsValue = returnArr;
            return byteReoccuring;
        }

        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            //this.ModelBytePairsValues = (from feature in featureValues as IFeatureCollectionWrapper<BytePairsReoccuringBase>
            //    group feature by feature.FeatureBytePairsValue
            //    into grp
            //    select grp.Key).Where(grp => grp != null).ToArray();

            var featuresTyped = featureValues as IFeatureCollectionWrapper<BytePairsReoccuringBase>;
            var hashAndCounts = featuresTyped.GroupBy(feature => feature.FeatureBytePairsValue, new ArrayEqualityComparer<byte>()).Select(g => new Entropy.EntropyItem<byte[]>
            {
                Key = g.Key,
                Count = g.Count()
            }).ToArray();

            this.ModelBytePairsValues = hashAndCounts.Select(hashAndCount => hashAndCount.Key).ToArray();

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
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
using AppIdent.Misc;
using AppIdent.Models;
using Framework.Models;
using Netfox.Core.Enums;
using PacketDotNet;

namespace AppIdent.Features.Bases
{
    public class TransportProtocolType : MandatoryFeatureBase
    {
        public TransportProtocolType() { }

        public TransportProtocolType(L7Conversation l7Conversation) : base(l7Conversation, DaRFlowDirection.non) { }

        public TransportProtocolType(double featureValue) : base(featureValue) { }

        public override FeatureKind FeatureKind { get; } = FeatureKind.Discrete;
        public virtual FeatureValueRange FeatureValueRange { get; } = new FeatureValueRange(0, 256);

        public IPProtocolType L4ProtocolType { get; set; }

        public override double ComputeDistanceToProtocolModel(FeatureBase sampleFeature)
        {
            return this.L4ProtocolType == (sampleFeature as TransportProtocolType)?.L4ProtocolType? 0 : 1;
        }

        #region Overrides of FeatureBase
        public override double ComputeFeature(L7Conversation l7Conversation, DaRFlowDirection flowDirection)
        {
            this.L4ProtocolType = l7Conversation.L4ProtocolType;
            return (double) this.L4ProtocolType;
        }
        #endregion

        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            var firstSample = featureValues.FirstOrDefault() as TransportProtocolType;
            foreach(var featureValue in featureValues)
            {
                if(((TransportProtocolType) featureValue).L4ProtocolType != firstSample.L4ProtocolType)
                    throw new InvalidOperationException($"Mandatory condition have not been met: {this.GetType().Name}");
            }
            this.L4ProtocolType = firstSample.L4ProtocolType;
            this.FeatureValue = (double) this.L4ProtocolType;
            this.Weight = 1;
        }
    }
}
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
using AppIdent.Misc;
using AppIdent.Models;
using Framework.Models;
using Netfox.Core.Enums;

namespace AppIdent.Features.Bases
{
    public class First3BytesEqualBase : FeatureBase
    {
        public First3BytesEqualBase() { }
        public First3BytesEqualBase(L7Conversation l7Conversation, DaRFlowDirection flowDirection) : base(l7Conversation, flowDirection) { }
        public First3BytesEqualBase(double featureValue) : base(featureValue) { }

        public override FeatureKind FeatureKind { get; } = FeatureKind.Discrete;
        public override FeatureValueRange FeatureValueRange { get; } = new FeatureValueRange(0, 1);
        public override double ComputeDistanceToProtocolModel(FeatureBase sampleFeature) { return this.FeatureValue.Equals(sampleFeature.FeatureValue)? 0 : 1; }

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


            var l7Pdus = pdus as L7PDU[] ?? pdus.ToArray();
            if(l7Pdus.Count() <= 1) { return 0; }

            var numBytes = 3;
            var firstPDUBytesCount = l7Pdus.First().PDUByteArr.Length;

            if(firstPDUBytesCount < numBytes) { numBytes = firstPDUBytesCount; }

            var pattern = l7Pdus.First().PDUByteArr.Take(numBytes).ToArray();

            foreach(var pdu in l7Pdus)
            {
                if(pdu.PDUByteArr.Length < numBytes) { return 0; }

                if(pattern.SequenceEqual(pdu.PDUByteArr.Take(3))) { continue; }
                return 0;
            }
            return 1;
        }

        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            var trueValues = featureValues.Count(feature => feature.FeatureValue.Equals(1.0));
            var falseValues = featureValues.Count(feature => feature.FeatureValue.Equals(0.0));

            this.FeatureValue = (trueValues > falseValues)? 1 : 0;

            if(trueValues == 0 || falseValues == 0) { this.Weight = 1; } // /(double)Math.Sqrt(trueValues+falseValues); }
            else if(trueValues < falseValues) { this.Weight = 1 - (trueValues / (double) falseValues); }
            else { this.Weight = 1 - (falseValues / (double) trueValues); }
        }
    }
}
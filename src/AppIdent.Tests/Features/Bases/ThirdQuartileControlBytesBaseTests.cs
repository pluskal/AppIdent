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
using System.Net;
using System.Threading.Tasks;
using AppIdent.Features.Bases;
using NetfoxFrameworkAPI.Tests.Properties;
using Netfox.Core.Enums;
using NUnit.Framework;

namespace AppIdent.Tests.Features.Bases
{
    [TestFixture]
    public class ThirdQuartileControlBytesBaseTests : FeatureBaseTests<ThirdQuartileControlBytesBase>
    {
        [Test]
        public override void ComputeDistanceToProtocolModelTest_TrainingToTesingDistance_ExpectedDistance()
        {
            this.ComputeDistanceToProtocolModelTest_TrainingToTesingDistance_NotZero(0.0d);
        }

        [Test]
        public override void ComputeFeature_FeatureValueDownFlow_ExpectedFeatureValue()
        {
            var feature = this.ComputeFeature_FeatureValue_ExpectedFeatureValue(DaRFlowDirection.down, 20);
        }

        #region Overrides of FeatureBaseTests<ThirdQuartileControlBytesBase>
        public override void ComputeFeature_FeatureValueNonFlow_ExpectedFeatureValue() { }
        #endregion

        [Test]
        public override void ComputeFeature_FeatureValueUpFlow_ExpectedFeatureValue()
        {
            var feature = this.ComputeFeature_FeatureValue_ExpectedFeatureValue(DaRFlowDirection.up, 20);
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            this.OneTimeSetUp(SnoopersPcaps.Default.features_three_conver_putty_ssh_cap);

            this.L7ConversationTesting = this.GetL7ConversationForIp(new IPEndPoint(IPAddress.Parse("192.168.1.102"), 21253));
            this.L7ConversationTraining1 = this.GetL7ConversationForIp(new IPEndPoint(IPAddress.Parse("192.168.1.102"), 21263));
            this.L7ConversationTraining2 = this.GetL7ConversationForIp(new IPEndPoint(IPAddress.Parse("192.168.1.102"), 21273));
        }

        #region Overrides of FeatureBaseTests<ThirdQuartileControlBytesBase>
        #endregion
    }
}
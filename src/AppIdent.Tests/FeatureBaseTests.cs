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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AppIdent.Features.Bases;
using AppIdent.Tests.Features.Bases;
using Framework.Models;
using Netfox.Core.Enums;
using NetfoxFrameworkAPI.Tests;
using NUnit.Framework;

namespace AppIdent.Tests
{
    public abstract class FeatureBaseTests<TFeature> : FrameworkBaseTests where TFeature : FeatureBase
    {
        protected L7Conversation L7ConversationTesting { get; set; }
        protected L7Conversation L7ConversationTraining1 { get; set; }
        protected L7Conversation L7ConversationTraining2 { get; set; }

        [Test]
        public void ComputeDistanceToProtocolModel_SelfDistance_Zero()
        {
            var feature = this.ComputeFeature(this.L7ConversationTesting, DaRFlowDirection.up);
            var distance = feature.ComputeDistanceToProtocolModel(feature);
            AssertDistanceValue(feature, distance, 0);
        }

        public abstract void ComputeDistanceToProtocolModelTest_TrainingToTesingDistance_ExpectedDistance();

        public void ComputeDistanceToProtocolModelTest_TrainingToTesingDistance_NotZero(double expectedDistance)
        {
            const DaRFlowDirection direction = DaRFlowDirection.up;
            var featureTraining1 = this.ComputeFeature(this.L7ConversationTraining1, direction);
            var featureTraining2 = this.ComputeFeature(this.L7ConversationTraining2, direction);
            var modelFeature = this.CreateFeature(direction);
            var featureCollection = new FeatureCollectionWrapperMock<TFeature>(new[]
            {
                featureTraining1,
                featureTraining2
            });
            modelFeature.InitializeNormalization(featureCollection);
            modelFeature.ComputeFeatureForProtocolModel(featureCollection);

            var featureTesting = this.ComputeFeature(this.L7ConversationTesting, direction);

            var distance = modelFeature.ComputeDistanceToProtocolModel(featureTesting);
            AssertDistanceValue(modelFeature, distance, expectedDistance);
        }

        public TFeature ComputeFeature_FeatureValue_ExpectedFeatureValue(DaRFlowDirection daRFlowDirection, double expectedValue)
        {
            var feature = this.ComputeFeature(this.L7ConversationTesting, daRFlowDirection);
            AssertComputedFeatureMLValue(feature, expectedValue);
            return feature;
        }

        public abstract void ComputeFeature_FeatureValueDownFlow_ExpectedFeatureValue();

        public abstract void ComputeFeature_FeatureValueNonFlow_ExpectedFeatureValue();

        public abstract void ComputeFeature_FeatureValueUpFlow_ExpectedFeatureValue();

        protected static void AssertComputedFeatureMLValue(FeatureBase feature, double expectedValue)
        {
            Assert.AreEqual(expectedValue, feature.FeatureValue, $"{feature.GetType().Name} - ComputeFeature returned incorrect value in {feature.FlowDirection} direction.");
        }

        protected static void AssertDistanceValue(FeatureBase feature, double distance, double expectedValue)
        {
            Assert.AreEqual(expectedValue, distance, $"{feature.GetType().Name} - ComputeDistanceToProtocolModel returned incorrect value in {feature.FlowDirection} direction.");
        }

        protected TFeature ComputeFeature(L7Conversation l7Conversation, DaRFlowDirection direction)
        {
            return Activator.CreateInstance(typeof(TFeature), l7Conversation, direction) as TFeature;
        }

        protected FeatureBase CreateFeature(DaRFlowDirection direction)
        {
            var feature = Activator.CreateInstance(typeof(TFeature)) as FeatureBase;
            Debug.Assert(feature != null, "feature != null");
            feature.FlowDirection = direction;
            return feature;
        }

        protected L7Conversation GetL7ConversationForIp(IPEndPoint ipEndPoint) { return this.L7Conversations.First(c => c.SourceEndPoint.Equals(ipEndPoint)); }

        protected virtual void OneTimeSetUp(string pcapFilePath)
        {
            this.SetUpInMemory();
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(pcapFilePath));
        }
        [SetUp]
        public void SetUp() { base.SingleTestWaitOne(); }
    }
}
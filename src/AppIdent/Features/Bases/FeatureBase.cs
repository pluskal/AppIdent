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
using LinqStatistics;
using Netfox.Core.Enums;

namespace AppIdent.Features.Bases
{
    [DataContract]
    public abstract class FeatureBase
    {
        private double _weight;
        private double _weightNormalized;
        protected FeatureBase() { }
        protected FeatureBase(double featureValue) : this() { this.FeatureValue = featureValue; }

        protected FeatureBase(L7Conversation l7Conversation, DaRFlowDirection flowDirection) : this()
        {
            this.FlowDirection = flowDirection;
            this.FeatureValue = this.ComputeFeature(l7Conversation, flowDirection);
        }

        public abstract FeatureKind FeatureKind { get; }
        public virtual FeatureValueRange FeatureValueRange { get; } = new FeatureValueRange(-1,1);
        public DaRFlowDirection FlowDirection { get; set; }

        public double NormalizedValue => this.Normalize(this.FeatureValue);

        public string Name => this.GetType().Name;

        [DataMember]
        public double FeatureValue { get; set; }

        [DataMember]
        public double[] ModelValues { get; set; }

        [DataMember]
        public double Weight
        {
            get => this._weight;
            set
            {
                this._weight = value * this.WeightBias;
                if(double.IsNaN(value)) { throw new InvalidOperationException("Weight cannot be NaN"); }
            }
        }

        public virtual double WeightBias { get; } = 1;

        [DataMember]
        public double NormalizedWeight
        {
            get => this._weightNormalized;
            set
            {
                this._weightNormalized = value;
                if(double.IsNaN(value)) { throw new InvalidOperationException("Weight cannot be NaN"); }
            }
        }

        [DataMember]
        public double Mean { get; protected set; }

        [DataMember]
        public double StdDev { get; protected set; }

        [DataMember]
        public double Max { get; protected set; }

        [DataMember]
        public double Min { get; protected set; }

        public double PositiveSigma => this.Mean + this.StdDev;
        public double NegativeSigma => this.Mean - this.StdDev;

        public virtual double ComputeDistanceToProtocolModel(FeatureBase sampleFeature)
        {
            if(sampleFeature.FeatureValue.Equals(-1.0) || !this.ModelValues.Any()) { return 0; }

            return this.Normalize(this.ModelValues.Min(featureValue => Math.Abs(featureValue - sampleFeature.FeatureValue)));
        }

        public abstract double ComputeFeature(L7Conversation l7Conversation, DaRFlowDirection flowDirection);

        /// <summary>
        ///     Computes FeatureValue and Weight for a feature.
        ///     In special case, more precise comparison using custome metric and implemtation might be used
        /// </summary>
        /// <param name="featureValues"></param>
        public virtual void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            this.ModelValues = featureValues.Cast<double>().ToArray();
        }

        public virtual void InitializeNormalization(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            var features = featureValues.Where(f => f.FeatureValue != -1.0);
            if(!features.Any())
            {
                this.Min = 0;
                this.Max = 0;
                this.Mean = 0;
                this.StdDev = 0;
                return;
            }
            var minMax = features.MinMax(feature => feature.FeatureValue);
            this.Min = minMax.Min;
            this.Max = minMax.Max;
            if(features.Count() < 2)
            {
                this.StdDev = 0;
                this.Mean = features.First().FeatureValue;
            }
            else
            {
                this.StdDev = features.StandardDeviation(feature => feature.FeatureValue);
                this.Mean = features.Average(feature => feature.FeatureValue);
            }
        }

        public static implicit operator double(FeatureBase feature) { return feature.FeatureValue; }
        public static implicit operator FeatureBase(double d) { return new FeatureBaseTemplate(d); }

        #region Overrides of Object
        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return
                $"{this.GetType().Name}: {this.FeatureValue}, NormalizedWeight: {this.NormalizedWeight}, Weight: {this.Weight}, MIN: {this.Min}, MAX: {this.Max}, Mean: {this.Mean}, StdDev: {this.StdDev}";
        }
        #endregion

        /// <summary>
        ///     https://en.wikipedia.org/wiki/Feature_scaling#Rescaling
        ///     To [0,1]
        /// </summary>
        /// <param name="featureValue"></param>
        /// <returns></returns>
        protected virtual double Normalize(double featureValue)
        {
            if(this.Min.Equals(this.Max)) { return 1; }
            return (featureValue - this.Min) / (this.Max - this.Min);
        }

        /// <summary>
        ///     https://en.wikipedia.org/wiki/Feature_scaling#Standardization
        ///     //TODO not used yet
        /// </summary>
        /// <param name="featureValue"></param>
        /// <returns></returns>
        protected virtual double Standardization(double featureValue)
        {
            return (featureValue - this.Mean) / this.StdDev;
        }

        private class FeatureBaseTemplate : FeatureBase
        {
            public FeatureBaseTemplate(double featureValue) : base(featureValue) { }
            public override FeatureKind FeatureKind { get; } = FeatureKind.Continous;
            public override double ComputeFeature(L7Conversation l7Conversation, DaRFlowDirection flowDirection) { throw new NotImplementedException(); }
            public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues) { throw new NotImplementedException(); }
        }
    }
}
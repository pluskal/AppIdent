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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AppIdent.Accord;
using AppIdent.Features.Bases;
using AppIdent.Misc;
using AppIdent.Models;

namespace AppIdent.EPI
{
    [DataContract]
    public class EPIProtocolModel
    {
        private Dictionary<string, FeatureBase> _featureVectorValues;
        private ConcurrentBag<FeatureVector> _trainingFeatureVectors = new ConcurrentBag<FeatureVector>();

        public EPIProtocolModel() { }

        public EPIProtocolModel(string applicationProtocolName, FeatureVector trainingFeatureVector, FeatureSelector featureSelector)
        {
            this.ApplicationProtocolName = applicationProtocolName;
            this.FeatureSelector = featureSelector;
            this.AddTrainingFeatureVector(trainingFeatureVector);
            //this.FeatureVectorProperties = typeof(FeatureVector).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(FeatureStatisticalAttribute))).ToList();
            this.FeatureVectorProperties = typeof(FeatureVector).GetProperties().Where(prop => this.FeatureSelector.SelectedFeatures.Contains(prop.PropertyType)).ToList();
        }

        [DataMember]
        public string ApplicationProtocolName { get; internal set; }

        public FeatureSelector FeatureSelector { get; }

        [DataMember]
        public FeatureVector ModelFeatureVector { get; internal set; }

        public Dictionary<string, FeatureBase> FeatureVectorValues
        {
            get
            {
                if(this._featureVectorValues == null)
                {
                    this._featureVectorValues = new Dictionary<string, FeatureBase>();
                    foreach(var propertyInfo in this.FeatureVectorProperties)
                    {
                        var modelFeature = propertyInfo.GetValue(this.ModelFeatureVector, null) as FeatureBase;
                        if(modelFeature == null) { throw new InvalidCastException("Not all FeatureVectors are based on FeatureBase - {info.Name}"); }

                        this._featureVectorValues.Add(modelFeature.GetType().Name, modelFeature);
                    }
                }
                return this._featureVectorValues;
            }
        }

        private IEnumerable<PropertyInfo> FeatureVectorProperties { get; set; }

        public void AddTrainingFeatureVector(FeatureVector trainingFeatureVector)
        {
            if(this._trainingFeatureVectors != null) { this._trainingFeatureVectors.Add(trainingFeatureVector); }
            else { throw new InvalidOperationException("It is not possible to add new training vectors, weights have been already computed"); }
        }

        public void ComputeApplicationProtocolModel()
        {
            this.ComputeModel();
            this._trainingFeatureVectors = null;
        }

        /// <summary>
        ///     Euclidean distance - https://en.wikipedia.org/wiki/Euclidean_distance
        /// </summary>
        /// <param name="featureVector"></param>
        /// <returns></returns>
        public double ComputeDistanceToProtocolModel(EPIProtocolModel secondModel)
        {
            var sum = 0.0;
            foreach(var propertyInfo in this.FeatureVectorProperties)
            {
                var firstFeature = propertyInfo.GetValue(this.ModelFeatureVector, null) as FeatureBase;
                var secondFeature = propertyInfo.GetValue(secondModel.ModelFeatureVector, null) as FeatureBase;
                if(firstFeature == null || secondFeature == null) { throw new InvalidCastException("Not all FeatureVectors are based on FeatureBase - {info.Name}"); }
                var diff = Math.Abs(firstFeature.FeatureValue - secondFeature.FeatureValue);
                sum += Math.Pow(diff, 2);
            }
            return Math.Sqrt(sum);
        }

        /// <summary>
        ///     Euclidean distance - https://en.wikipedia.org/wiki/Euclidean_distance
        /// </summary>
        /// <param name="featureVector"></param>
        /// <returns></returns>
        public double ComputeDistanceToProtocolModelValue(FeatureVector featureVector)
        {
            var differencesFeatureVector = this.ComputeDistanceToProtocolModel(featureVector);

            var sum = 0.0;
            foreach(var propertyInfo in this.FeatureVectorProperties)
            {
                var featureDiff = propertyInfo.GetValue(differencesFeatureVector, null) as FeatureBase;
                var modelFeature = propertyInfo.GetValue(this.ModelFeatureVector, null) as FeatureBase;
                if(featureDiff == null || modelFeature == null) { throw new InvalidCastException("Not all FeatureVectors are based on FeatureBase - {info.Name}"); }

                if(typeof(MandatoryFeatureBase).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    if(featureDiff != 0) return double.MaxValue;
                    continue;
                }
                sum += Math.Pow(featureDiff.FeatureValue, 2) * modelFeature.NormalizedWeight;
            }
            return Math.Sqrt(sum);
        }

        public void SetFeatureVectorProperties()
        {
            this.FeatureVectorProperties = typeof(FeatureVector).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(FeatureStatisticalAttribute))).ToList();
        }

        private FeatureVector ComputeDistanceToProtocolModel(FeatureVector sampleFeatureVector) //todo rename to more suited name
        {
            var metricDifferences = new FeatureVector();

            Parallel.ForEach(this.FeatureVectorProperties, propertyInfo =>
            {
                try
                {
                    var sampleFeature = propertyInfo.GetValue(sampleFeatureVector, null) as FeatureBase;
                    if(sampleFeature == null) { throw new InvalidCastException("Not all FeatureVectors are based on FeatureBase - {info.Name}"); }

                    var modelFeature = propertyInfo.GetValue(this.ModelFeatureVector, null) as FeatureBase;
                    if(modelFeature == null) { throw new InvalidCastException("Not all FeatureVectors are based on FeatureBase - {info.Name}"); }

                    var difference = modelFeature.ComputeDistanceToProtocolModel(sampleFeature);

                    propertyInfo.SetValue(metricDifferences, Activator.CreateInstance(propertyInfo.PropertyType, difference));
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    Debugger.Break();
                }
            });
            return metricDifferences;
        }

        private void ComputeModel()
        {
            var trainingFeatureVectors = this._trainingFeatureVectors.ToArray();
            if(!trainingFeatureVectors.Any()) { throw new InvalidOperationException("No training feature vectors present!"); }

            this.ModelFeatureVector = new FeatureVector
            {
                Label = this.ApplicationProtocolName
            };

            var localLockObject = new object();
            var sumOfWeights = 0.0;
            Parallel.ForEach(this.FeatureVectorProperties, // source collection
                info => // body
                {
                    var feature = info.GetValue(this.ModelFeatureVector, null) as FeatureBase;
                    if(feature == null) { throw new InvalidCastException("Not all FeatureVectors are based on FeatureBase - " + info.Name); }

                    var featureCollectionType = typeof(FeatureCollectionWrapper<>).MakeGenericType(feature.GetType());
                    var features = Activator.CreateInstance(featureCollectionType, new[]
                    {
                        trainingFeatureVectors
                    });

                    feature.ComputeFeatureForProtocolModel(features as IFeatureCollectionWrapper<FeatureBase>);
                    feature.InitializeNormalization(features as IFeatureCollectionWrapper<FeatureBase>);

                    lock(localLockObject) { sumOfWeights += feature.Weight; }
                });

            Parallel.ForEach(this.FeatureVectorProperties, info =>
            {
                var feature = info.GetValue(this.ModelFeatureVector, null) as FeatureBase;
                if(feature == null) { throw new InvalidCastException("Not all FeatureVectors are based on FeatureBase - " + info.Name); }
                if(sumOfWeights != 0) feature.NormalizedWeight = feature.Weight / sumOfWeights;
                else
                {
                    feature.NormalizedWeight = 0;
                    //Debugger.Break();
                }
            });
        }

        #region Overrides of Object
        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.AppendLine($"Name: {this.ApplicationProtocolName}");


            foreach(var info in this.FeatureVectorProperties)
            {
                var feature = info.GetValue(this.ModelFeatureVector, null) as FeatureBase;
                if(feature == null) { throw new InvalidCastException("Not all FeatureVectors are based on FeatureBase - " + info.Name); }

                sb.AppendLine(feature.ToString());
            }
            return sb.ToString();
        }

        public string ToCsvHeaderString()
        {
            var sb = new StringBuilder();

            sb.Append($"Name,");

            foreach(var info in this.FeatureVectorProperties)
            {
                var feature = info.GetValue(this.ModelFeatureVector, null) as FeatureBase;
                if(feature == null) { throw new InvalidCastException("Not all FeatureVectors are based on FeatureBase - " + info.Name); }

                sb.Append($"{feature.GetType().Name},");
            }
            return sb.ToString();
        }

        public string ToCsvString()
        {
            var sb = new StringBuilder();

            sb.Append($"{this.ApplicationProtocolName.Split('.').Last()},");

            foreach(var info in this.FeatureVectorProperties)
            {
                var feature = info.GetValue(this.ModelFeatureVector, null) as FeatureBase;
                if(feature == null) { throw new InvalidCastException("Not all FeatureVectors are based on FeatureBase - " + info.Name); }

                sb.Append($"{feature.FeatureValue},");
            }
            return sb.ToString();
        }
        #endregion
    }
}
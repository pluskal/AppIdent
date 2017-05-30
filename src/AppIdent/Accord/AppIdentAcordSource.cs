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
using System.Reflection;
using System.Threading.Tasks;
using Accord;
using Accord.MachineLearning.DecisionTrees;
using AppIdent.Features.Bases;
using AppIdent.Misc;
using AppIdent.Models;

namespace AppIdent.Accord
{
    public class AppIdentAcordSource
    {
        //public AppIdentAcordSource()
        //{
        //    this.FeatureSelector = new FeatureSelector(this.GetFeatureVectorFeatureProperties().Select(p => p.PropertyType));
        //}

        public AppIdentAcordSource(FeatureSelector featureSelector) { this.FeatureSelector = featureSelector; }
        public FeatureSelector FeatureSelector { get;  }

        private string[] _labels;
        private int[] _labelsAsIntegers;

        public string[] Labels => this._labels ?? (this._labels = this.FeatureVectors.Select(featureVector => featureVector.Label).ToArray());
        public string[] LabelsFromInteges => this._labelsFromInteges ?? (this._labelsFromInteges = this.FeatureVectors.Select(featureVector => featureVector.Label).Distinct().ToArray());

        //public double[][] Samples => this._samples ?? (this._samples = this.FeatureVectors.Select(this.GetFeatureVectorValue).ToArray());
        public double[][] Samples =>  this.FeatureVectors.Select(this.GetFeatureVectorValue).ToArray();

        //public double[,] Samples2D => this._samples2D ?? (this._samples2D = this.ComputeSamples2D(this.FeatureVectors));
        public double[,] Samples2D =>this.ComputeSamples2D(this.FeatureVectors);
        public int[] LabelsAsIntegers => this._labelsAsIntegers ?? (this._labelsAsIntegers = this.LabelsToints(this.Labels));
        private DecisionVariable[] _decisionVariables;
        private string[] _labelsFromInteges;

        public DecisionVariable[] DecisionVariables
        {
            get
            {
                lock(this) { return this._decisionVariables ?? (this._decisionVariables = this.GetDecisionVariables()); }
            }
        }

        private DecisionVariable[] GetDecisionVariables()
        {
            var decisionVariables = new List<DecisionVariable>();
            var featureVector = this.FeatureVectors.First();

            Parallel.ForEach(this.GetFeatureVectorFilteredFeatureProperties(), // source collection
                info => // body
                {
                    var feature = info.GetValue(featureVector, null) as FeatureBase;
                    if(feature == null) { throw new InvalidCastException("Not all FeatureVectors are based on FeatureBase - " + info.Name); }

                    var featureCollectionType = typeof(FeatureCollectionWrapper<>).MakeGenericType(feature.GetType());
                    var featureVectors = this.FeatureVectors.ToArray();
                    var features = Activator.CreateInstance(featureCollectionType, new[]
                    {
                        featureVectors
                    });

                    feature.ComputeFeatureForProtocolModel(features as IFeatureCollectionWrapper<FeatureBase>);
                    feature.InitializeNormalization(features as IFeatureCollectionWrapper<FeatureBase>);
                });

            foreach(var featureProperty in this.GetFeatureVectorFilteredFeatureProperties())
            {
                var feature = featureProperty.GetValue(featureVector) as FeatureBase;
                //todo dicrete ommited because of AccordBug
                //var decisionVariableKind = feature.FeatureKind == FeatureKind.Continous? DecisionVariableKind.Continuous : DecisionVariableKind.Discrete;
                var decisionVariableKind = DecisionVariableKind.Continuous;
                var decisionVariable = new DecisionVariable(feature.Name, decisionVariableKind)
                {
                    Range = new DoubleRange(feature.Min, feature.Max)
                };
                decisionVariables.Add(decisionVariable);
            }
            return decisionVariables.ToArray();
        }

        public string[] FeatureNames => this.GetFeatureVectorFilteredFeatureProperties().Select(p => p.Name).ToArray();
        private IEnumerable<FeatureVector> FeatureVectors { get; set; }

        public void Init(IEnumerable<FeatureVector> featureVectors) { this.FeatureVectors = featureVectors; }

        private double[,] ComputeSamples2D(IEnumerable<FeatureVector> featureVectors)
        {
            var featureVectorsArray = featureVectors as FeatureVector[] ?? featureVectors.ToArray();
            var featurePropertiesCount = this.GetFeatureVectorFilteredFeatureProperties().Count();
            var samples2D = new double[featureVectorsArray.Length, featurePropertiesCount];
            foreach(var sample in this.Samples.Select((x, i) => new
            {
                value = x,
                index = i
            })) { for(var i = 0; i < featurePropertiesCount; i++) { samples2D[sample.index, i] = sample.value[i]; } }
            return samples2D;
        }

        private  IEnumerable<PropertyInfo> GetFeatureVectorFeatureProperties()
        {
            return typeof(FeatureVector).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(FeatureStatisticalAttribute)));
        }

        private IEnumerable<PropertyInfo> GetFeatureVectorFilteredFeatureProperties()
        {
            return this.GetFeatureVectorFeatureProperties().Where(prop => this.FeatureSelector.SelectedFeatures.Contains(prop.PropertyType));
        }

        private double[] GetFeatureVectorValue(FeatureVector featureVector)
        {
            var properties = this.GetFeatureVectorFilteredFeatureProperties();
            var propertyValues = properties.Select(property => property.GetValue(featureVector)).Cast<FeatureBase>();
            return propertyValues.Select(pv => (double) pv.FeatureValue).ToArray();
        }


        public int[] LabelsToints(string[] listOfStringLabels)
        {
            var intLabels = new int[listOfStringLabels.Length];
            var dictOfLabels = new Dictionary<string, int>();
            foreach (var label in listOfStringLabels.Distinct()
                .ToList()
                .Select((x, i) => new
                {
                    val = x,
                    ind = i
                })) { dictOfLabels[label.val] = label.ind; }
            foreach (var label in listOfStringLabels.Select((x, i) => new
            {
                val = x,
                ind = i
            })) { intLabels[label.ind] = dictOfLabels[label.val]; }
            return intLabels;
        }
    }
}
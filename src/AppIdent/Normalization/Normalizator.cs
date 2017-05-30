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
using AppIdent.Features.Bases;
using AppIdent.Models;

namespace AppIdent.Normalization
{
    public class Normalizator
    {
        public FeatureVector Mean { get; internal set; }

        public FeatureVector StdDev { get; internal set; }
        public void Initialize(FeatureVector[] stats) { this.ComputeMeanAndStdDeviation(stats); }

        public void Normalize(FeatureVector[] stats)
        {
            foreach(var stat in stats)
            {
                foreach(var property in stat.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(FeatureStatisticalAttribute))))
                {
                    var name = property.Name;
                    var feature = property.GetValue(stat, null);
                    var propertyInfo = feature.GetType().GetProperty(nameof(FeatureBase.FeatureValue));
                    var propertyValue = (double) propertyInfo.GetValue(feature, null);
                    if(!propertyValue.Equals(-1.0))
                    {
                        propertyInfo.SetValue(feature, Utilities.Z_score(propertyValue, this.GetPropValue(this.Mean, name), this.GetPropValue(this.StdDev, name)), null);
                    }
                }
            }
        }

        public void Normalize(FeatureVector stat)
        {
            var t = stat.GetType();
            foreach(var property in t.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(FeatureStatisticalAttribute))))
            {
                var name = property.Name;
                var feature = property.GetValue(stat, null);
                var propertyInfo = feature.GetType().GetProperty(nameof(FeatureBase.FeatureValue));
                var propertyValue = (double) propertyInfo.GetValue(feature, null);
                if(!propertyValue.Equals(-1.0))
                {
                    propertyInfo.SetValue(feature,
                        Utilities.Z_score((double) propertyInfo.GetValue(feature, null), this.GetPropValue(this.Mean, name), this.GetPropValue(this.StdDev, name)), null);
                }
            }
        }

        private void ComputeMeanAndStdDeviation(FeatureVector[] stats)
        {
            this.Mean = new FeatureVector();
            this.StdDev = new FeatureVector();

            //roztrideni statistik, <statistika, seznam hodnot>
            var tmpDict = new Dictionary<string, List<double>>();

            var t = stats.First().GetType();
            foreach(var stat in stats)
            {
                foreach(var property in t.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(FeatureStatisticalAttribute))))
                {
                    var name = property.Name;
                    var tmpProperty = property.GetValue(stat, null);
                    if(!tmpDict.ContainsKey(name)) { tmpDict.Add(name, new List<double>()); }
                    var propertyValue = (double) tmpProperty.GetType().GetProperty(nameof(FeatureBase.FeatureValue)).GetValue(tmpProperty);
                    if(!propertyValue.Equals(-1.0)) { tmpDict[name].Add(propertyValue); }
                }
            }

            //TODO paralel? 
            foreach(var property in t.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(FeatureStatisticalAttribute))))
            {
                var name = property.Name;

                property.SetValue(this.Mean, Activator.CreateInstance(property.PropertyType, Utilities.MeanValue(tmpDict[name])), null);
                property.SetValue(this.StdDev, Activator.CreateInstance(property.PropertyType, Utilities.StandardDeviation(tmpDict[name])), null);
            }
        }

        private double GetPropValue(object src, string propName)
        {
            var tmpProperty = src.GetType().GetProperty(propName).GetValue(src, null);
            return (double) tmpProperty.GetType().GetProperty(nameof(FeatureBase.FeatureValue)).GetValue(tmpProperty);
        }
    }
}
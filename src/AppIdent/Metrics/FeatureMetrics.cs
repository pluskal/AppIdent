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
using AppIdent.Features.Bases;
using AppIdent.Misc;
using AppIdent.Models;

namespace AppIdent.Metrics
{
    public static class FeatureMetrics
    {
        public static double DistanceMetric(FeatureBase value, IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            if(value.FeatureValue.Equals(-1.0) || !featureValues.Any()) { return 0; }
            var result = Math.Abs(value.FeatureValue - featureValues.First().FeatureValue);
            foreach(var stat in featureValues.Skip(1))
            {
                var d = Math.Abs(value.FeatureValue - stat.FeatureValue);
                if(d < result) { result = d; }
            }
            //Console.WriteLine("Distance " + result + " value: " + value.FeatureValue);
            return result;
        }

        public static double FeatureMetricAverage(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            var features = featureValues.Where(feature => !feature.FeatureValue.Equals(-1.0)).ToArray();
            return !features.Any()? 0 : features.Average(feature => feature.FeatureValue);
        }

        public static double First3BEqualMetric(FeatureBase value, IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            if(value.FeatureValue.Equals(-1.0) || !featureValues.Any()) { return 0; }
            var tmpTrue = featureValues.Count(x => x.FeatureValue.Equals(1.0));
            var tmpFalse = featureValues.Count(x => x.FeatureValue.Equals(0.0));
            Console.WriteLine("3Bequal " + tmpTrue + " " + tmpFalse + " FeatureValue: " + value.FeatureValue);
            if((tmpTrue > tmpFalse && value.FeatureValue.Equals(1.0)) || (tmpTrue < tmpFalse && value.FeatureValue.Equals(0.0))) { return 0.0; }

            return 1.0;
        }

        public static double IsValueInStatsMetric(FeatureBase value, IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            if(value.FeatureValue.Equals(-1.0) || !featureValues.Any()) { return 0; }

            return featureValues.Any(feature => value.FeatureValue.Equals(feature.FeatureValue))? 0.0 : 1.0;
        }
    }
}
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

namespace AppIdent
{
    public static class Utilities
    {
        public static double F_Measure(int tp, int fp, int p)
        {
            var prec = Precission(tp, fp);
            var rec = Recall(tp, p);
            if(prec == 0 || rec == 0) { return 0; }
            var res = 2 * prec * rec / (prec + rec);
            return res;
        }

        public static double MeanValue(List<double> valueList)
        {
            var tmpValue = 0.0;
            var count = 0;
            foreach(var value in valueList)
            {
                if(value.Equals(-1.0)) { continue; }
                tmpValue += value;
                count++;
            }
            return tmpValue / count;
        }

        public static double Precission(int tp, int fp)
        {
            if(tp + fp == 0) { return 0; }
            var res = (double) tp / (tp + fp);
            return res;
        }

        public static double Recall(int tp, int p)
        {
            if(p == 0) { return 0; }
            var res = (double) tp / p;
            return res;
        }

        //http://stackoverflow.com/questions/895929/how-do-i-determine-the-standard-deviation-stddev-of-a-set-of-values
        public static double StandardDeviation(List<int> valueList)
        {
            var M = 0.0;
            var S = 0.0;
            var k = 1;
            foreach(double value in valueList)
            {
                var tmpM = M;
                M += (value - tmpM) / k;
                S += (value - tmpM) * (value - M);
                k++;
            }
            return Math.Sqrt(S / (k - 1));
        }

        public static double StandardDeviation(List<long> valueList)
        {
            var M = 0.0;
            var S = 0.0;
            var k = 1;
            foreach(double value in valueList)
            {
                if(value.Equals(-1.0)) { continue; }
                var tmpM = M;
                M += (value - tmpM) / k;
                S += (value - tmpM) * (value - M);
                k++;
            }
            return Math.Sqrt(S / (k - 1));
        }

        public static double StandardDeviation(List<double> valueList)
        {
            var M = 0.0;
            var S = 0.0;
            var k = 1;
            foreach(var value in valueList)
            {
                if(value.Equals(-1.0)) { continue; }
                var tmpM = M;
                M += (value - tmpM) / k;
                S += (value - tmpM) * (value - M);
                k++;
            }
            return Math.Sqrt(S / (k - 1));
        }

        public static double Z_score(double oldValue, double mean, double stdDev)
        {
            if(stdDev != 0) { return (oldValue - mean) / stdDev; }

            return 0;
        }

        public static double Z_score(double oldValue, List<double> stats)
        {
            var stdDev = StandardDeviation(stats);

            if(stdDev != 0) { return (oldValue - stats.Average()) / stdDev; }

            return 0;
        }
    }
}
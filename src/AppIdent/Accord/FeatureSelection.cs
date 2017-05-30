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
using Accord.Statistics;

namespace AppIdent.Accord
{
    public class FeatureSelection
    {
        private FeatureSelector FeatureSelector { get; set; }
        private double[,] GetCorrelationMatrix(AppIdentAcordSource appIdentAcordSource) => appIdentAcordSource.Samples2D.Correlation();

        private Type GetMostCorellatedFeature(double[,] correlationMatrix, out double correlation)
        {
            var lineMaxs = new List<double>();
            for(var i = 0; i < correlationMatrix.GetLength(0); i++)
            {
                double maxLine = 0;
                for(var j = 0; j < correlationMatrix.GetLength(1); j++)
                {
                    var abs = Math.Abs(correlationMatrix[i, j]);
                    if(i == j) continue;
                    maxLine = abs > maxLine? abs : maxLine;
                }
                lineMaxs.Add(maxLine);
            }
            correlation = lineMaxs.Max();
            var lineIndex = lineMaxs.IndexOf(correlation);
            var columnIndex = lineMaxs.LastIndexOf(correlation);

            var line = this.TransformAbs(this.GetLine(correlationMatrix, lineIndex));
            var column = this.TransformAbs(this.GetColumn(correlationMatrix, columnIndex));

            var lineMean = line.Mean();
            var columnMean = column.Mean();
            return lineMean > columnMean? this.FeatureSelector.SelectedFeatures[lineIndex] : this.FeatureSelector.SelectedFeatures[columnIndex];
        }

        private double[] GetColumn(double[,] correlationMatrix, int index)
        {
            var column = new double[correlationMatrix.GetLength(1)];
            for (var i = 0; i < correlationMatrix.GetLength(1); i++)
            {
                column[i]=correlationMatrix[index, i];
               
            }
            return column;
        }

        private double[] GetLine(double[,] correlationMatrix, int index)
        {
            var line = new double[correlationMatrix.GetLength(0)];
            for (var i = 0; i < correlationMatrix.GetLength(0); i++)
            {
                line[i] = correlationMatrix[i, index];

            }
            return line;
        }

        private double[] TransformAbs(double[] array) => array.Select(Math.Abs).ToArray();

        public IEnumerable<double[,]> ProcessFeatureSelection(AppIdentAcordSource appIdentAcordSource, double trashold)
        {
            this.FeatureSelector = appIdentAcordSource.FeatureSelector;
            this.RemoveFeaturesWithConstantValue(appIdentAcordSource);

            var iterationResults = new List<double[,]>();
            Type mostCorrelatedFeature = null;
            double correlation = 1;
            while (correlation > trashold)
            {
                if(mostCorrelatedFeature != null) appIdentAcordSource.FeatureSelector.RemoveFeature(mostCorrelatedFeature);
                var correlationMatrix = this.GetCorrelationMatrix(appIdentAcordSource);
                iterationResults.Add(correlationMatrix);
                mostCorrelatedFeature = this.GetMostCorellatedFeature(correlationMatrix, out correlation);
            }
            this.FeatureSelector = null;
            return iterationResults;
        }

        private void RemoveFeaturesWithConstantValue(AppIdentAcordSource appIdentAcordSource)
        {
            var matrix = appIdentAcordSource.Samples2D;
            var sampleCount = matrix.GetLength(0);
            var featureCount = matrix.GetLength(1);
            var removedFeatures = 0;
            for(var featureIndex = 0; featureIndex < featureCount; featureIndex++)
            {
                var isConstant = true;
                var firstValue = matrix[0, featureIndex];
                for(var sampleIndex = 1; sampleIndex < sampleCount; sampleIndex++)
                {
                    if(!(Math.Abs(firstValue - matrix[sampleIndex, featureIndex]) > 0)) continue;
                    isConstant = false;
                    break;
                }
                if(isConstant)
                {
                    var feature = this.FeatureSelector.SelectedFeatures[featureIndex - removedFeatures];
                    removedFeatures++;
                    appIdentAcordSource.FeatureSelector.RemoveFeature(feature);
                }
            }
        }
    }
}
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
using OxyPlot.Axes;
using OxyPlot.Series;

namespace CorrelationMatrixChart
{
//    using System.Collections.Generic;
    using OxyPlot;

    public class MainViewModel
    {
        //public MainViewModel(string[] featureNameArray, double[][] correlationMatrixData)
        public MainViewModel()
        {
            
        }
            public MainViewModel(string[] featureNameArray, double[,] correlationMatrixData)
        {
            this.CorrelationMatrixHeatMap = this.CreateModel(featureNameArray, correlationMatrixData);

        }
        
        public PlotModel CorrelationMatrixHeatMap { get;
            set;
        }

        private  PlotModel CreateModel(string[] featureNameArray, double[,] correlationMatrixData)
        {
            var model = new PlotModel { Title = "Correlation Matrix of Features" };
            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Key = "BottomFeatureAxis",
                ItemsSource = featureNameArray,
                Angle = -45,
                //FontSize = 20
            });
            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                Key = "LeftFeatureAxis",
                ItemsSource = featureNameArray,
                //FontSize = 20
            });
            model.Axes.Add(new LinearColorAxis
            {
                Minimum = -1,
                Maximum = 1,
                //Palette = OxyPalettes.BlueWhiteRed(200)
                Palette = OxyPalettes.BlueWhiteRed(200)
//                Palette = OxyPalettes.Hot(200)
            });
            var heatMapSeries = new HeatMapSeries
            {
                X0 = 0,
                X1 = featureNameArray.Length,
                Y0 = 0,
                Y1 = featureNameArray.Length,
                XAxisKey = "BottomFeatureAxis",
                YAxisKey = "LeftFeatureAxis",
                RenderMethod = HeatMapRenderMethod.Rectangles,
                LabelFontSize = 0.3, // neccessary to display the 
                Data = correlationMatrixData
            };
            model.Series.Add(heatMapSeries);
            return model;
        }
    }
}
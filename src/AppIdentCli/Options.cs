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
using System.Text;
using System.Threading.Tasks;
using Accord.MachineLearning;
using CommandLine;
using CommandLine.Text;

namespace AppIdentCli
{
    public class Options
    {
        [Option('d', "data-source", Required = true, HelpText = "AppIdentDataSource serialized data source file path.")]
        public string AppIdentDataSource { get; set; }

        [Option('f', "random-forest", HelpText = "Use random forest classification.")]
        public bool IsRandomForest { get; set; }
        [Option('p', "best-parameters", HelpText = "BestParameters bin file.")]
        public string BestParametersFilePath { get; set; }
        [Option('b', "bayesian", HelpText = "Use bayesian classification.")]
        public bool IsBayesian { get; set; }

        [Option('e', "epi", HelpText = "Use EPI classification.")]
        public bool IsEpi { get; set; }

        [Option('r', "ratio", HelpText = "Trainnig to verification ratio.")]
        public double TrainingToVerificationRation { get; set; } = 0.7;
        [Option('m', "min-flows", HelpText = "Minimum flows for training and classification.")]
        public int MinFlows { get; set; } = 30;

        [Option('s', "feature-selection", HelpText = "Feature corelation trashold <0-1>.")]
        public double FeatureSelectionTrashold { get; set; } = 2;
        [Option('c', "cross-validation-folds", HelpText = "Cross validation folds (only RF)")]
        public int CrossValidationFolds { get; set; } = 2;
        [Option('n', "use-full-name", HelpText = "Use application protocol full name including application name.")]
        public bool IsUseFullName { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}

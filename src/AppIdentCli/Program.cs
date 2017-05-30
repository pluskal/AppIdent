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
using System.IO;
using System.Linq;
using Accord.MachineLearning;
using AppIdent;
using AppIdent.EPI;
using AppIdent.Misc;
using AppIdent.Models;
using CommandLine;

namespace AppIdentCli
{
    class Program
    {
        private static void DebuggerCheckExit()
        {
            if(Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to continue . . .");
                Console.ReadLine();
            }
        }

        private static void Epi(AppIdentService appIdentService, AppIdentDataSource appIdentDataSource, AppIdentTestContext context)
        {
            Console.WriteLine($"{DateTime.Now} Running feature elimination with trashold { context.FeatureSelectionTreshold}.");
            var featureSelector = appIdentService.EliminateCorelatedFeatures(appIdentDataSource, context.FeatureSelectionTreshold, context);

            Console.WriteLine($"{DateTime.Now} Running classification");
            var classificationStatisticsMeter = appIdentService.EpiClasify(appIdentDataSource, featureSelector, context);

            Console.WriteLine($"{DateTime.Now} Classification results:");
            classificationStatisticsMeter.PrintResults();
        }

        static void Main(string[] args)
        {
            var watch = new Stopwatch();
            watch.Start();
            
            var options = new Options();
            if(!Parser.Default.ParseArguments(args, options))
            {
                DebuggerCheckExit();
                return;
            }

            //options.TrainingToVerificationRation = 0.7;
            //options.FeatureSelectionTrashold = 0.7;
            //options.IsEpi = true;
            //options.IsUseFullName = false;
            //options.MinFlows = 17;

            if(!options.IsEpi && !options.IsRandomForest && !options.IsBayesian)
            {
                Console.WriteLine("No classification method selected.");
                DebuggerCheckExit();
                return;
            }

            var context = new AppIdentTestContext("cli", DateTime.Now)
            {
                MinFlows = options.MinFlows,
                TrainingToVerificationRation = options.TrainingToVerificationRation,
                FeatureSelectionTreshold = options.FeatureSelectionTrashold,
                CrossValidationFolds = options.CrossValidationFolds,
                IsEpi = options.IsEpi,
                IsRandomForest = options.IsRandomForest,
                IsBayesian = options.IsBayesian,
                IsUseFullName = options.IsUseFullName,
                BestParametersFilePath = options.BestParametersFilePath,
            };
            context.ChangeNameByParameters();
            context.Save();

            SetLabelType(options);

            Console.WriteLine($"{DateTime.Now} Loading: {options.AppIdentDataSource}");
            var appIdentDataSource = context.LoadAppIdentDataSource(options.AppIdentDataSource);

            Console.WriteLine($"{DateTime.Now} Repartitioning ratio {context.TrainingToVerificationRation} with min flows {context.MinFlows}");
            appIdentDataSource.RepartitionFeatureVectorsTestingAndVerificationDatasets(context.TrainingToVerificationRation, context.MinFlows);
            context.SavePartitioning(appIdentDataSource);
            context.AppIdentDataSource = appIdentDataSource;

            if(!appIdentDataSource.TrainingSet.Any() || !appIdentDataSource.VerificationSet.Any())
            {
                Console.WriteLine("No testing or verification data present after partitioning and filtering by minflow.");
                DebuggerCheckExit();
                return;
            }

            var appIdentService = new AppIdentService();

            if(options.IsRandomForest) { RandomForest(appIdentService, appIdentDataSource, context); }
            if(options.IsEpi) { Epi(appIdentService, appIdentDataSource, context); }
            if(options.IsBayesian) { Bayesian(appIdentService, appIdentDataSource, context); }

            watch.Stop();
            context.RunningTime = watch.Elapsed;
            context.Save();

            
            Console.WriteLine($"{DateTime.Now} Running time: {watch.Elapsed}");

            DebuggerCheckExit();


        }

        private static void Bayesian(AppIdentService appIdentService, AppIdentDataSource appIdentDataSource, AppIdentTestContext context)
        {
            Console.WriteLine($"{DateTime.Now} Running feature elimination with trashold { context.FeatureSelectionTreshold}.");
            var featureSelector = appIdentService.EliminateCorelatedFeatures(appIdentDataSource, context.FeatureSelectionTreshold, context);

            Console.WriteLine($"{DateTime.Now} Running classification.");
            var classificationStatisticsMeter = appIdentService.BayesianClassify(appIdentDataSource, context.TrainingToVerificationRation, 0.99, context);

            Console.WriteLine($"{DateTime.Now} Classification results:");
            classificationStatisticsMeter.PrintResults();
            context.Save();
        }

        private static void RandomForest(AppIdentService appIdentService, AppIdentDataSource appIdentDataSource, AppIdentTestContext context)
        {
            Console.WriteLine($"{DateTime.Now} Running feature elimination with trashold { context.FeatureSelectionTreshold}.");
            var featureSelector = appIdentService.EliminateCorelatedFeatures(appIdentDataSource, context.FeatureSelectionTreshold, context);

            GridSearchParameterCollection bestParameters;
            if(!File.Exists(context.BestParametersFilePath))
            {
                Console.WriteLine($"{DateTime.Now} Looking for best parameters.");
                bestParameters = appIdentService.RandomForestGetBestParameters(appIdentDataSource, featureSelector, context);
            }
            else
            {
                Console.WriteLine($"{DateTime.Now} Loading best parameters.");
                context.Load<GridSearchParameterCollection>(context.BestParametersFilePath, out bestParameters);
            }

            Console.WriteLine($"{DateTime.Now} Running cross validation.");
            var classificationStatisticsMeter =
                appIdentService.RandomForestCrossValidation(appIdentDataSource, context.FeatureSelector, bestParameters, context.CrossValidationFolds, context);

            Console.WriteLine($"{DateTime.Now} Cross validation results:");
            classificationStatisticsMeter.PrintResults();

            var model = context.Model;

            Console.WriteLine($"{DateTime.Now} Running classification");
            appIdentService.AccordClassify(appIdentDataSource, model, context.FeatureSelector, context);

            Console.WriteLine($"{DateTime.Now} Classification results:");
            classificationStatisticsMeter.PrintResults();
            context.Save();
        }

        private static void SetLabelType(Options options)
        {
            LabelSelector.LabelType = options.IsUseFullName? LabelSelector.ELabelType.ApplicationProtocolNameFull : LabelSelector.ELabelType.ApplicationProtocolName;
        }
    }
}
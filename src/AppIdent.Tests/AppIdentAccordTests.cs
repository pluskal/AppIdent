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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Accord.IO;
using Accord.MachineLearning.DecisionTrees;
using Accord.Math;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Analysis;
using AppIdent.Accord;
using AppIdent.EPI;
using AppIdent.Misc;
using AppIdent.Statistics;
using CorrelationMatrixChart;
using Framework.Models;
using NetfoxFrameworkAPI.Tests;
using NetfoxFrameworkAPI.Tests.Properties;
using NUnit.Framework;

//using Vector = numl.Math.LinearAlgebra.Vector;

namespace AppIdent.Tests
{
    [TestFixture]
    public class AppIdentAccordTests : FrameworkBaseTests
    {
        public AppIdentAccordTests()
        {
            this.AccordAppIdent = new AccordAppIdent();
        }


        public AccordAppIdent AccordAppIdent { get; }

        public bool OmmitClassifiedModelDetails => this.L7Conversations.Count() > 1000;

        public AppIdentService AppIdentService { get; set; } = new AppIdentService();


        [Test]
        //[DotMemoryUnit(CollectAllocations = true)]
        //[AssertTraffic(AllocatedSizeInBytes = 1024*1024)]
        public void CorrelationMatrixTest_M2()
        {
            MainWindow window = null;
            this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_dnsHttpTls_cap);
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_learn1_cap);
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_refSkype_cap);
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_streamSkypeHttpTls_cap);
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_testM1_cap);
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_testM2_cap);
            var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations);
            var featureVectors = appIdentDataSource.TrainingSet;
            var appIdentAcordSource = new AppIdentAcordSource(new FeatureSelector());
            appIdentAcordSource.Init(featureVectors);

            var featureSelection = new FeatureSelection();

            var selection = featureSelection.ProcessFeatureSelection(appIdentAcordSource, 0.3);
            var correlationMatrix = selection.Last();


            // The dispatcher thread
            var t = new Thread(() =>
            {
                window = new MainWindow
                {
                    DataContext = new MainViewModel(appIdentAcordSource.FeatureNames, correlationMatrix)
                };
                //window.DataContext = new MainViewModel();
                // Initiates the dispatcher thread shutdown when the window closes
                window.Closed += (s, e) => window.Dispatcher.InvokeShutdown();
                window.Show();
                // Makes the thread support message pumping
                Dispatcher.Run();
            });
            // Configure the thread
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }

        [Test]
        public void DecisionTreeTest_M2()
        {
            Console.WriteLine("\n##### Decision Tree with 30% threshold #####\n");
            this.DecisionTreeTestCliBase(SnoopersPcaps.Default.app_identification_testM2_cap, 0.3);
            Console.WriteLine("\n##### Decision Tree with 70% threshold #####\n");
            this.DecisionTreeTestCliBase(SnoopersPcaps.Default.app_identification_testM2_cap, 0.7);
            Console.WriteLine("\n##### Decision Tree with 90% threshold #####\n");
            this.DecisionTreeTestCliBase(SnoopersPcaps.Default.app_identification_testM2_cap, 0.9);
        }



        public void ProcessPcapFile(string pcapFilePath) { this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(pcapFilePath)); }

        [Test]
        public void RandomForestsCrossValidationTest_M2()
        {
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_dnsHttpTls_cap);
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_learn1_cap);
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_refSkype_cap);
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_streamSkypeHttpTls_cap);
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_testM1_cap);
            //this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_testM2_cap);
            this.ProcessPcapFile(@"D:\pcaps\AppIdent-TestingData\captured\pc_13\20-4\data0.cap");
            this.ProcessPcapFile(@"D:\pcaps\AppIdent-TestingData\captured\pc_14\20-4\data0.cap");
            this.ProcessPcapFile(@"D:\pcaps\AppIdent-TestingData\captured\pc_15\20-4\data0.cap");
            this.ProcessPcapFile(@"D:\pcaps\AppIdent-TestingData\captured\pc_16\20-4\data0.cap");
            this.ProcessPcapFile(@"D:\pcaps\AppIdent-TestingData\captured\pc_17\20-4\data0.cap");

            var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations, 30);
            var featureVectors = appIdentDataSource.TrainingSet;
            var appIdentAcordSource = new AppIdentAcordSource(new FeatureSelector());
            appIdentAcordSource.Init(featureVectors);

            Console.WriteLine($"Feature vectors: {featureVectors.Length}");
            var featureSelection = new FeatureSelection();
            featureSelection.ProcessFeatureSelection(appIdentAcordSource, 0.5);

            foreach(var feature in appIdentAcordSource.FeatureSelector.SelectedFeatures)
            {
                Console.WriteLine($"{feature.Name}");
            }

            var rfcModel = this.AccordAppIdent.GetBestRandomForestsWithGridSearch(appIdentAcordSource, out var bestParameters, out var minError);
            // get the cross validation results
            var cvResults = this.AccordAppIdent.GetCrossValidationResultsOfRandomForestModel(appIdentAcordSource, bestParameters);
            Console.WriteLine("### CV Results ###");
            Console.WriteLine("\n### Training stats ###");
            Console.WriteLine(">> model error mean: {0}\n>> model std:  {1}", Math.Round(cvResults.Training.Mean, 6), Math.Round(cvResults.Training.StandardDeviation, 6));
            Console.WriteLine("\n### Validation stats ###");
            Console.WriteLine(">> model error mean: {0}\n>> model std:  {1}", Math.Round(cvResults.Validation.Mean, 6), Math.Round(cvResults.Validation.StandardDeviation, 6));

            var minErorr = cvResults.Validation.Values.Min();
            var bestIndex = cvResults.Validation.Values.IndexOf(minErorr);
            var classifier = cvResults.Models[bestIndex];

            var validationDataSource = classifier.Tag as AccordAppIdent.ValidationDataSource;
            var predictedValues = classifier.Model.Decide(validationDataSource.ValidationInputs);

            var i = 0;
            foreach (var label in appIdentAcordSource.Labels.Distinct())
            {
                var conf = new ConfusionMatrix(validationDataSource.ValidationOutputs, predictedValues, i++);
                Console.WriteLine($"##########################");
                Console.WriteLine($"Protocol: {label}");
                Console.WriteLine($"TP: {conf.TruePositives}");
                Console.WriteLine($"TN: {conf.TrueNegatives}");
                Console.WriteLine($"FN: {conf.FalseNegatives}");
                Console.WriteLine($"FP: {conf.FalsePositives}");
                Console.WriteLine($"Accuracy: {conf.Accuracy}");
                Console.WriteLine($"Precision: {conf.Precision}");
                Console.WriteLine($"Specificity: {conf.Specificity}");
                Console.WriteLine($"Sensitivity: {conf.Sensitivity}");
                Console.WriteLine($"FScore: {conf.FScore}");
            }
        }

        [Test]
        public void RandomForestsCrossValidationTest_2()
        {
            this.ProcessPcapFile(SnoopersPcaps.Default.app_identification_dnsHttpTls_cap);

            var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations, 10);
            var featureVectors = appIdentDataSource.TrainingSet;
            var appIdentAcordSource = new AppIdentAcordSource(new FeatureSelector());
            appIdentAcordSource.Init(featureVectors);

            Console.WriteLine($"Feature vectors: {featureVectors.Length}");
            var featureSelection = new FeatureSelection();
            featureSelection.ProcessFeatureSelection(appIdentAcordSource, 0.5);

            foreach (var feature in appIdentAcordSource.FeatureSelector.SelectedFeatures)
            {
                Console.WriteLine($"{feature.Name}");
            }

            this.AccordAppIdent.GetBestRandomForestsWithGridSearch(appIdentAcordSource, out var bestParameters, out var minError);
            // get the cross validation results
            var cvResults = this.AccordAppIdent.GetCrossValidationResultsOfRandomForestModel(appIdentAcordSource, bestParameters);
            Console.WriteLine("### CV Results ###");
            Console.WriteLine("\n### Training stats ###");
            Console.WriteLine(">> model error mean: {0}\n>> model std:  {1}", Math.Round(cvResults.Training.Mean, 6), Math.Round(cvResults.Training.StandardDeviation, 6));
            Console.WriteLine("\n### Validation stats ###");
            Console.WriteLine(">> model error mean: {0}\n>> model std:  {1}", Math.Round(cvResults.Validation.Mean, 6), Math.Round(cvResults.Validation.StandardDeviation, 6));

            var minErorr = cvResults.Validation.Values.Min();
            var bestIndex = cvResults.Validation.Values.IndexOf(minErorr);
            var classifier = cvResults.Models[bestIndex];

            var labels = appIdentAcordSource.Labels.Distinct();

            var modelStore = new AppIdentTestContext();
            var model = classifier.Model;
            var randomForestFilePath =  modelStore.Save(model, labels);
            modelStore.Load<RandomForest>(randomForestFilePath, out var randomForestModel, out var labelsLoaded);


            var validationDataSource = classifier.Tag as AccordAppIdent.ValidationDataSource;
            var predictedValues = randomForestModel.Decide(validationDataSource.ValidationInputs);

            var i = 0;
            foreach (var label in labelsLoaded)
            {
                var conf = new ConfusionMatrix(validationDataSource.ValidationOutputs, predictedValues, i++);
                Console.WriteLine($"##########################");
                Console.WriteLine($"Protocol: {label}");
                Console.WriteLine($"TP: {conf.TruePositives}");
                Console.WriteLine($"TN: {conf.TrueNegatives}");
                Console.WriteLine($"FN: {conf.FalseNegatives}");
                Console.WriteLine($"FP: {conf.FalsePositives}");
                Console.WriteLine($"Accuracy: {conf.Accuracy}");
                Console.WriteLine($"Precision: {conf.Precision}");
                Console.WriteLine($"Specificity: {conf.Specificity}");
                Console.WriteLine($"Sensitivity: {conf.Sensitivity}");
                Console.WriteLine($"FScore: {conf.FScore}");
            }
        }



        [SetUp]
        public void SetUp()
        {
            this.SetUpInMemory();
        }

        protected void DecisionTreeTestCliBase(string pcapFilePath, double trainingToClassifyingRatio, double precisionTrashHold = 0.99)
        {
            var precMeasure = this.DecisionTreeTestBase(pcapFilePath, trainingToClassifyingRatio, precisionTrashHold);
            precMeasure.PrintResults();
        }


        // DECISION TREE CLF

        private ApplicationProtocolClassificationStatisticsMeter DecisionTreeTestBase(
            string pcapFilePath,
            double trainingToClassifyingRatio,
            double precisionTrashHold = 0.99,
            int minFlows = 1)
        {
            this.ProcessPcapFile(pcapFilePath);
            var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations, minFlows);
            return this.AppIdentService.DecisionTreeClassify(appIdentDataSource, trainingToClassifyingRatio, precisionTrashHold);
        }

        // TODO: not tested yet

       
        //                    // Create the multi-class learning algorithm for the machine
        //                    // double gamma = parameters["gamma"].Value;
        //                     double complexity = parameters["complexity"].Value;
        //                     double constant = parameters["constant"].Value;
        //                     int degree = (int)parameters["degree"].Value;
        //                    // The parameters to be tried will be passed as a function parameter.
        //                {
        //                Fitting = delegate (GridSearchParameterCollection parameters, out double error)
        //                // Set the fitting function for the algorithm
        //            {
        //            var gridsearch = new GridSearch<MultilabelSupportVectorLearning<Polynomial>>(parameterRanges)
        //            // Instantiate a new Grid Search algorithm for Kernel Support Vector Machines
        //            };
        //                new GridSearchRange("constant",   new double[] { 0, 1, 2 } )
        //                new GridSearchRange("degree",     new double[] { 1, 10, 2, 3, 4, 5 } ),
        //                    var teacher = new MultilabelSupportVectorLearning<Polynomial>
        //                new GridSearchRange("complexity", new double[] { 0.00000001, 0.20, 0.30, 0.50 } ),
        //            {
        //            GridSearchRange[] parameterRanges =
        //            // grid search ranges (parameter values)
        //        {

        //        private GridSearchParameterCollection GetBestParametersOfSVMWithGridSearch(double[][] samples, int[] labels)
        //                    {
        //                        // Configure the learning algorithm to use SMO to train the
        //                        //  underlying SVMs in each of the binary class subproblems.
        //                        Learner = (param) => new SequentialMinimalOptimization<Polynomial>()
        //                        {
        //                            Complexity = complexity
        //                        },
        //                        Kernel = new Polynomial(degree, constant),
        //                        ParallelOptions = { MaxDegreeOfParallelism = 1 }
        //                    };
        //                    var ksvm = teacher.Learn(samples, labels);
        //
        //                    // Obtain class predictions for each sample
        //                    var predicted = ksvm.Decide(samples);
        //
        //                    // Compute classification error using mean accuracy (mAcc)
        //                    error = new HammingLoss(labels).Loss(predicted);
        //                    return ksvm;
        //                }
        //            };

        //            // Create the multi-class learning algorithm for the machine
        //            var teacher = new MultilabelSupportVectorLearning<Gaussian>()
        //            {
        //                // Configure the learning algorithm to use SMO to train the
        //                //  underlying SVMs in each of the binary class subproblems.
        //                Learner = (param) => new SequentialMinimalOptimization<Gaussian>()
        //                {
        //                    // Estimate a suitable guess for the Gaussian kernel's parameters.
        //                    // This estimate can serve as a starting point for a grid search.
        //                    UseKernelEstimation = true
        //                }
        //            };
        //
        //            // Learn a machine
        //            var machine = teacher.Learn(inputs, outputs);
        //
        //            // Create the multi-class learning algorithm for the machine
        //            var calibration = new MultilabelSupportVectorLearning<Gaussian>()
        //            {
        //                Model = machine, // We will start with an existing machine
        //
        //                // Configure the learning algorithm to use SMO to train the
        //                //  underlying SVMs in each of the binary class subproblems.
        //                Learner = (param) => new ProbabilisticOutputCalibration<Gaussian>()
        //                {
        //                    Model = param.Model // Start with an existing machine
        //                }
        //            };
        //
        //
        //            // Configure parallel execution options
        //            calibration.ParallelOptions.MaxDegreeOfParallelism = 1;
        //
        //            // Learn a machine
        //            calibration.Learn(inputs, outputs);
        //
        //            // Obtain class predictions for each sample
        //            bool[][] predicted = machine.Decide(inputs);
        //
        //            // Get class scores for each sample
        //            double[][] scores = machine.Scores(inputs);
        //
        //            // Get log-likelihoods (should be same as scores)
        //            double[][] logl = machine.LogLikelihoods(inputs);
        //
        //            // Get probability for each sample
        //            double[][] prob = machine.Probabilities(inputs);
        //
        //            // Compute classification error using mean accuracy (mAcc)
        //            double error = new HammingLoss(outputs).Loss(predicted);
        //            double loss = new CategoryCrossEntropyLoss(outputs).Loss(prob);
        //             

        //            // Declare some out variables to pass to the grid search algorithm
        //            GridSearchParameterCollection bestParameters; double minError;
        //
        //            // Compute the grid search to find the best Support Vector Machine
        //            KernelSupportVectorMachine bestModel = gridsearch.Compute(out bestParameters, out minError);
        //
        //            // Compute the grid search to find the best RandomForest model
        //            var bestRfcModel = gridSearch.Compute(out bestParameters, out minError);
        //            Console.WriteLine("Min Error:");
        //            Console.WriteLine(minError);
        //            Console.WriteLine("Best Parameters:");
        //            Console.WriteLine(bestParameters.ToString());
        //            Console.WriteLine("### Looping through collection of parameters ###");
        //            foreach (var param in bestParameters) { Console.WriteLine("{0}: {1}", param.Name, param.Value); }
        //            return bestParameters;
        //        }
        //
        //        private CrossValidationResult<SupportVectorMachine> GetCrossValidationResultsOfSVMModel(double[][] samples, int[] labels, GridSearchParameterCollection bestParameters)
        //        {
        //
        //        }

        //[Test]
        //public void DecisionTreeCrossValidationTest_M2()
        //{
        //    // http://accord-framework.net/docs/html/T_Accord_MachineLearning_DecisionTrees_Learning_C45Learning.htm
        //    // get X and y from pcap file
        //    var res = this.GetSamplesAndLabelsFromFeatureVectors2(SnoopersPcaps.Default.app_identification_refSkype_cap);
        //    // samples (X) as double[][]
        //    var samples = res.Item2;
        //    // labels (y) as 
        //    var labels = this.labelsToints(res.Item1);
        //    // get the RFC model with the best hyper-parameter setup (number of trees, sample ratio)
        //    var bestParameters = this.GetBestParametersOfSVMWithGridSearch(samples, labels);
        //    // get the cross validation results
        //    var cvResults = this.GetCrossValidationResultsOfSVMModel(samples, labels, bestParameters);
        //    Console.WriteLine("### CV Results ###");
        //    Console.WriteLine("\n### Training stats ###");
        //    Console.WriteLine(">> model mean: {0}%\n>> model std:  {1}%", Math.Round(cvResults.Training.Mean * 100, 4), Math.Round(cvResults.Training.StandardDeviation * 100, 4));
        //    Console.WriteLine("\n### Validation stats ###");
        //    Console.WriteLine(">> model mean: {0}%\n>> model std:  {1}%", Math.Round(cvResults.Validation.Mean * 100, 4), Math.Round(cvResults.Validation.StandardDeviation * 100, 4));

        //}
    }
}
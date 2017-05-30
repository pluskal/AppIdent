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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using Accord.IO;
using Accord.MachineLearning;
using Accord.MachineLearning.DecisionTrees;
using Accord.Math;
using AppIdent.Accord;
using AppIdent.EPI;
using AppIdent.Misc;
using AppIdent.Models;
using AppIdent.NUML.Classifiers;
using AppIdent.Statistics;
using Framework.Models;

namespace AppIdent
{
    public class AppIdentService
    {
        public AppIdentDataSource CreateAppIdentDataSource(IEnumerable<L7Conversation> l7Conversations, int minFlows = 1, double trainingToClassifyingRatio = 1)
        {
            var appIdentDataSource = new AppIdentDataSource();
            appIdentDataSource.Initialize(l7Conversations, minFlows, trainingToClassifyingRatio);
            return appIdentDataSource;
        }

        //public void CreateDatasetAndTestset(
        //    AppIdentDataSource appIdentDataSource,
        //    double trainingToClassifyingRatio,
        //    out List<FeatureVector> trainingSet,
        //    out List<FeatureVector> verificationSet)
        //{
        //    trainingSet = new List<FeatureVector>();
        //    verificationSet = new List<FeatureVector>();

        //    var groupedFeatureVectors = from featureVector in appIdentDataSource.FeatureVectors
        //        group featureVector by featureVector.Label
        //        into featureVectors
        //        orderby featureVectors.Key
        //        select featureVectors;
        //    //todo this can me managed more randomly
        //    foreach(var gc in groupedFeatureVectors)
        //    {
        //        var conves = gc.ToList();
        //        var ratioIndex = (int) (conves.Count * trainingToClassifyingRatio);
        //        var testingDataCount = conves.Count - ratioIndex;
        //        trainingSet.AddRange(conves.GetRange(0, ratioIndex));
        //        verificationSet.AddRange(conves.GetRange(ratioIndex, testingDataCount));
        //    }
        //}

        public ApplicationProtocolClassificationStatisticsMeter DecisionTreeClassify(
            AppIdentDataSource appIdentDataSource,
            double trainingToVerificationRatio,
            double precisionTrashHold)
        {
            var precMeasure = new ApplicationProtocolClassificationStatisticsMeter();

            //this.CreateDatasetAndTestset(appIdentDataSource, trainingToVerificationRatio, out var trainingSet, out var verificationSet);
            var classifier = new DecisionTreeClassifier(appIdentDataSource.TrainingSet);

            foreach(var feature in appIdentDataSource.VerificationSet)
            {
                var appTag = feature.Label;
                feature.Label = "Unknown";
                classifier.Normalizator.Normalize(feature);
                var cl = classifier.ClassifierModel.Predict(feature);
                if(cl.Precision > precisionTrashHold) { precMeasure.UpdateStatistics(feature.Label, appTag); }
            }
            return precMeasure;
        }

        public FeatureSelector EliminateCorelatedFeatures(AppIdentDataSource appIdentDataSource, double trashold, AppIdentTestContext appIdentTestContext)
        {
            var appIdentAcordSource = this.GetAppIdentAcordSource(appIdentDataSource.TrainingSet, new FeatureSelector());
            return this.EliminateCorelatedFeatures(appIdentAcordSource, trashold, appIdentTestContext);
        }

        public FeatureSelector EliminateCorelatedFeatures(AppIdentAcordSource appIdentAcordSource, double trashold, AppIdentTestContext appIdentTestContext)
        {
            var featureSelection = new FeatureSelection();
            featureSelection.ProcessFeatureSelection(appIdentAcordSource, trashold);

            var featureSelector = appIdentAcordSource.FeatureSelector;
            appIdentTestContext.Save(featureSelector);
            return featureSelector;
        }

        public ApplicationProtocolClassificationStatisticsMeter EpiClasify(
            AppIdentDataSource appIdentDataSource,
            FeatureSelector featureSelector,
            out EPIEvaluator epiEvaluator,
            AppIdentTestContext appIdentTestContext = null)
        {
            //this.CreateDatasetAndTestset(appIdentDataSource, trainingToVerificationRatio, out var trainingSet, out var verificationSet);
            epiEvaluator = new EPIEvaluator(featureSelector);
            epiEvaluator.CreateApplicationProtocolModels(appIdentDataSource.TrainingSet);
            var precMeasure = epiEvaluator.ComputeStatistics(appIdentDataSource.VerificationSet);
            appIdentTestContext?.Save(precMeasure);
            return precMeasure;
        }

        public ApplicationProtocolClassificationStatisticsMeter EpiClasify(
            AppIdentDataSource appIdentDataSource,
            FeatureSelector featureSelector,
            AppIdentTestContext appIdentTestContext = null)
        {
            return this.EpiClasify(appIdentDataSource, featureSelector,  out var epiEvaluator, appIdentTestContext);
        }

        public ApplicationProtocolClassificationStatisticsMeter LoadStatisticsFromXml(string fileName)
        {
            var statisticsMeter = new ApplicationProtocolClassificationStatisticsMeter();
            try
            {
                var dcs = new DataContractSerializer(typeof(ApplicationProtocolClassificationStatisticsMeter));
                var fs = new FileStream(fileName, FileMode.Open);
                var reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());

                statisticsMeter = (ApplicationProtocolClassificationStatisticsMeter) dcs.ReadObject(reader);
                reader.Close();
                fs.Close();
                return statisticsMeter;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot Load model " + fileName);
                Console.WriteLine(ex);
                throw;
            }
        }

        public ApplicationProtocolClassificationStatisticsMeter BayesianClassify(AppIdentDataSource appIdentDataSource, double trainingToVerificationRatio, double precisionTrashHold, AppIdentTestContext appIdentTestContext = null)
        {
            var precMeasure = new ApplicationProtocolClassificationStatisticsMeter();

            //this.CreateDatasetAndTestset(appIdentDataSource, trainingToVerificationRatio, out var trainingSet, out var verificationSet);
            var classifier = new NaiveBayesClassifier(appIdentDataSource.TrainingSet);

            foreach(var featureVector in appIdentDataSource.VerificationSet)
            {
                var appTag = featureVector.Label.Replace("_", "").Replace("-", "");

                featureVector.Label = "Unknown";
                classifier.Normalizator.Normalize(featureVector);
                var cl = classifier.ClassifierModel.Predict(featureVector, true);
                if(cl.Precision > precisionTrashHold) { precMeasure.UpdateStatistics(cl.Label, appTag); }
            }
            appIdentTestContext?.Save(precMeasure);
            return precMeasure;
        }

        public ApplicationProtocolClassificationStatisticsMeter RandomForestCrossValidation(
            AppIdentDataSource appIdentDataSource,
            FeatureSelector featureSelector,
            GridSearchParameterCollection bestParameters,
            int folds,
            AppIdentTestContext appIdentTestContext)
        {
            var precMeasure = new ApplicationProtocolClassificationStatisticsMeter();

            var accordAppIdent = new AccordAppIdent();
            var appIdentAcordSource = this.GetAppIdentAcordSource(appIdentDataSource.TrainingSet, featureSelector);

            var cvResults = accordAppIdent.GetCrossValidationResultsOfRandomForestModel(appIdentAcordSource, bestParameters, folds);
            Console.WriteLine("### CV Results ###");
            Console.WriteLine("\n### Training stats ###");
            Console.WriteLine(">> model error mean: {0}\n>> model std:  {1}", Math.Round(cvResults.Training.Mean, 6), Math.Round(cvResults.Training.StandardDeviation, 6));
            Console.WriteLine("\n### Validation stats ###");
            Console.WriteLine(">> model error mean: {0}\n>> model std:  {1}", Math.Round(cvResults.Validation.Mean, 6), Math.Round(cvResults.Validation.StandardDeviation, 6));

            var minErorr = cvResults.Validation.Values.Min();
            var bestIndex = cvResults.Validation.Values.IndexOf(minErorr);
            var classifier = cvResults.Models[bestIndex];

            var model = classifier.Model;
            var labels = appIdentAcordSource.Labels.Distinct();
            var modelFilePath = appIdentTestContext.Save(model, labels);

            var validationDataSource = classifier.Tag as AccordAppIdent.ValidationDataSource;
            var predictedValues = classifier.Model.Decide(validationDataSource.ValidationInputs);

            for(var j = 0; j < predictedValues.Length; j++)
            {
                precMeasure.UpdateStatistics(appIdentAcordSource.LabelsFromInteges[predictedValues[j]],
                    appIdentAcordSource.LabelsFromInteges[validationDataSource.ValidationOutputs[j]]);
            }
            appIdentTestContext.SaveCrossValidation(precMeasure);

            return precMeasure;
        }

        public ApplicationProtocolClassificationStatisticsMeter AccordClassify(
            AppIdentDataSource appIdentDataSource,
            MulticlassClassifierBase model,
            FeatureSelector featureSelector,
            AppIdentTestContext appIdentTestContext)
        {
            var precMeasure = new ApplicationProtocolClassificationStatisticsMeter();
            var appIdentAcordSource = this.GetAppIdentAcordSource(appIdentDataSource.VerificationSet, featureSelector);
            var predictedValues = model.Decide(appIdentAcordSource.Samples);
            for (var j = 0; j < predictedValues.Length; j++)
            {
                precMeasure.UpdateStatistics(appIdentAcordSource.LabelsFromInteges[predictedValues[j]],appIdentAcordSource.Labels[j]);
            }
            appIdentTestContext.Save(precMeasure);
            return precMeasure;
        }


        public GridSearchParameterCollection RandomForestGetBestParameters(
            AppIdentDataSource appIdentDataSource,
            FeatureSelector featureSelector,
            AppIdentTestContext appIdentTestContext = null)
        {
            var accordAppIdent = new AccordAppIdent();
            var appIdentAcordSource = this.GetAppIdentAcordSource(appIdentDataSource.TrainingSet, featureSelector);

            accordAppIdent.GetBestRandomForestsWithGridSearch(appIdentAcordSource, out var bestParameters, out var minError);
            appIdentTestContext?.Save(bestParameters);
            return bestParameters;
        }

        public void SaveStatisticsToxml(string fileName, ApplicationProtocolClassificationStatisticsMeter applicationStaticticsMeter)
        {
            try
            {
                var serializer = new DataContractSerializer(typeof(ApplicationProtocolClassificationStatisticsMeter));
                var writer = XmlWriter.Create(fileName);
                serializer.WriteObject(writer, applicationStaticticsMeter);
                writer.Close();
            }
            catch(Exception ex) { Console.WriteLine("EXCEPTION during serialization\n" + ex); }
        }

        private AppIdentAcordSource GetAppIdentAcordSource(FeatureVector[] featureVectors, FeatureSelector featureSelector)
        {
            var appIdentAcordSource = new AppIdentAcordSource(featureSelector);
            appIdentAcordSource.Init(featureVectors);
            return appIdentAcordSource;
        }
    }
}
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AppIdent.Accord;
using AppIdent.EPI;
using AppIdent.Models;
using AppIdent.NUML.Classifiers;
using AppIdent.Statistics;
using Framework.Models;
using Framework.Models.PmLib.Captures;
using Framework.Models.PmLib.Frames;
using Netfox.Core.Database;
using NetfoxFrameworkAPI.Tests;
using NetfoxFrameworkAPI.Tests.Properties;
using NUnit.Framework;

//using Catharsis.Commons;
namespace AppIdent.Tests
{
        [TestFixture]
        public class ApplicationRecognizerAppIDentTest : FrameworkBaseTests
        {

            public bool OmmitClassifiedModelDetails => this.L7Conversations.Count() > 1000;

            public void ProcessPcapFile(string pcapFilePath) { this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(pcapFilePath)); }

            public ApplicationProtocolClassificationStatisticsMeter EpiTestBase(
                string pcapFilePath,
                double trainingToClassifyingRatio,
                out EPIEvaluator epiEvaluator,
                int minFlows = 1)
            {
                this.ProcessPcapFile(pcapFilePath);
                var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations,minFlows, trainingToClassifyingRatio);
               return this.AppIdentService.EpiClasify(appIdentDataSource, new FeatureSelector(), out  epiEvaluator);
            }

            public ApplicationProtocolClassificationStatisticsMeter MLTestBase(
                string pcapFilePath,
                double trainingToClassifyingRatio,
                double precisionTrashHold = 0.99,
                int minFlows = 1)
            {
                this.ProcessPcapFile(pcapFilePath);
                var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations, minFlows);
                return  this.AppIdentService.BayesianClassify(appIdentDataSource,trainingToClassifyingRatio,precisionTrashHold);
               
            }
        
            [SetUp]
            public void SetUp()
            {
                base.SetUpInMemory();
            }

            public AppIdentService AppIdentService { get; set; } = new AppIdentService();

            [Test]
            public void CompareStatisticsTest()
            {
                this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.app_identification_testM2_cap));
                var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations);
                var precMeasure = this.AppIdentService.EpiClasify(appIdentDataSource, new FeatureSelector());
                precMeasure.PrintResults();
                this.CompareStatistics(precMeasure, "testMeasure.xml");

            }

            [Test]
            public void EPIselfTest()
            {
                this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.app_identification_testM2_cap));


                var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations);

                var epiEvaluator = new EPIEvaluator(new FeatureSelector());
                epiEvaluator.CreateApplicationProtocolModels(appIdentDataSource.TrainingSet);
                var precMeasure = epiEvaluator.ComputeStatistics(appIdentDataSource.TrainingSet);

                var consoleDefaultColor = Console.ForegroundColor;
            Console.ForegroundColor = consoleDefaultColor;
                Console.WriteLine("################# Procotol model details: ####################");
                epiEvaluator.PrintProtocolModels();

                Console.WriteLine("################# Procotol similarities: ####################");
                epiEvaluator.AgregateProtocolModels();

                Console.WriteLine("################# Summary: ####################");
                precMeasure.PrintResults();
                //applicationProtocolModelEvaluator.PrintCsvProtocolModels();

                this.AppIdentService.SaveStatisticsToxml("testMeasure.xml", precMeasure);
            }

          
            protected EPIEvaluator EpiTestCliBase(string pcapFilePath, double trainingToClassifyingRatio)
            {
                var precMeasure = this.EpiTestBase(pcapFilePath, trainingToClassifyingRatio, out var epiEvaluator);

                Console.WriteLine("################# Procotol model details: ####################");
                epiEvaluator.PrintProtocolModels();

                Console.WriteLine("################# Procotol similarities: ####################");
                epiEvaluator.AgregateProtocolModels();

                Console.WriteLine("################# Summary: ####################");
                precMeasure.PrintResults();

                //this.SaveModelForMeasure(applicationProtocolModelEvaluator, precMeasure);
                //applicationProtocolModelEvaluator.PrintCsvProtocolModels();

                Console.WriteLine("################# Application Protocol Models Hierachival Clustering: ####################");

                epiEvaluator.PrintClusters();
                return epiEvaluator;
            }

            [Test]
            public void EPItest30()
            {
                this.EpiTestCliBase(SnoopersPcaps.Default.app_identification_dnsHttpTls_cap, 0.3);
            }

            [Test]
            public void EPItest70()
            {
                this.EpiTestCliBase(SnoopersPcaps.Default.app_identification_dnsHttpTls_cap, 0.7);
            }

            [Test]
            public void EPItest90()
            {
                this.EpiTestCliBase(SnoopersPcaps.Default.app_identification_dnsHttpTls_cap, 0.9);
            }

            [Test]
            public void EPItest90M2()
            {
                this.EpiTestCliBase(SnoopersPcaps.Default.app_identification_testM2_cap, 0.9);
            }

   
            public EPIEvaluator MlEpiTestBase(
                string pcapFilePath,
                double trainingToClassifyingRatio,
                out ApplicationProtocolClassificationStatisticsMeter epiprecMeasure,
                out ApplicationProtocolClassificationStatisticsMeter mlprecMeasure,
                double precisionTrashHold,
                int minFlows = 1)
            {
                   this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.app_identification_testM2_cap));
                var appIdentDataSource = this.AppIdentService.CreateAppIdentDataSource(this.L7Conversations,minFlows,trainingToClassifyingRatio);
                EPIEvaluator epiEvaluator;
                epiprecMeasure = this.AppIdentService.EpiClasify(appIdentDataSource, new FeatureSelector(), out epiEvaluator);
                mlprecMeasure = this.AppIdentService.BayesianClassify(appIdentDataSource,0.7,precisionTrashHold);
                return epiEvaluator;
            }




            protected void MLTestCliBase(string pcapFilePath, double trainingToClassifyingRatio, double precisionTrashHold = 0.99)
            {
                var precMeasure = this.MLTestBase(pcapFilePath, trainingToClassifyingRatio, precisionTrashHold);
                precMeasure.PrintResults();
            }


            [Test]
            public void BayesNetworkTest_M2()
            {
                Console.WriteLine("\n##### Bayes Network with 30% threshold #####\n");
                this.MLTestCliBase(SnoopersPcaps.Default.app_identification_testM2_cap, 0.3);
                Console.WriteLine("\n##### Bayes Network with 70% threshold #####\n");
                this.MLTestCliBase(SnoopersPcaps.Default.app_identification_testM2_cap, 0.7);
                Console.WriteLine("\n##### Bayes Network with 90% threshold #####\n");
                this.MLTestCliBase(SnoopersPcaps.Default.app_identification_testM2_cap, 0.9);
            }




            [Test]
            public void MLtest30()
            {
                this.MLTestCliBase(SnoopersPcaps.Default.app_identification_testM2_cap, 0.3);
            }


            [Test]
            public void MLtest70()
            {
                this.MLTestCliBase(SnoopersPcaps.Default.app_identification_testM2_cap, 0.7);
            }

            [Test]
            public void MLtest90()
            {
                this.MLTestCliBase(SnoopersPcaps.Default.app_identification_testM2_cap, 0.9);
            }

            [Test]
            public void EPItestipluskal_appIdent_test()
            {
                this.EpiTestCliBase(@"D:\pcaps\pcpluskal2\appIdent_test.cap", 0.9);
            }

            [Test]
            public void MLtestipluskal_appIdent_test()
            {
                this.MLTestCliBase(@"D:\pcaps\pcpluskal2\appIdent_test.cap", 0.9);
            }

            [Test]
            public void EPItestipluskal_appIdent_test1()
            {
                this.EpiTestCliBase(@"D:\pcaps\pcpluskal2\appIdent_test1.cap", 0.9);
            }

            [Test]
            public void MLtestipluskal_appIdent_test1()
            {
                this.MLTestCliBase(@"D:\pcaps\pcpluskal2\appIdent_test1.cap", 0.9);
            }

            [Test]
            public void EPItestipluskal_appIdent_test2()
            {
                this.EpiTestCliBase(@"D:\pcaps\pcpluskal2\appIdent_test2.cap", 0.9);
            }

            [Test]
            public void MLtestipluskal_appIdent_test2()
            {
                this.MLTestCliBase(@"D:\pcaps\pcpluskal2\appIdent_test2.cap", 0.9);
            }

            [Test]
            public void EPItestipluskal_appIdent1()
            {
                this.EpiTestCliBase(@"D:\pcaps\pcpluskal2\appIdent1.cap", 0.9);
            }

            [Test]
            public void MLtestipluskal_appIdent1()
            {
                this.MLTestCliBase(@"D:\pcaps\pcpluskal2\appIdent1.cap", 0.9);
            }

            [Test]
            public void EPItestipluskal_pcr_20160415()
            {
                this.EpiTestCliBase(@"D:\pcaps\pcpluskal2\pcr-20160415.cap", 0.9);
            }

            [Test]
            public void MLtestipluskal_pcr_20160415()
            {
                this.MLTestCliBase(@"D:\pcaps\pcpluskal2\pcr-20160415.cap", 0.9);
            }

            [Test]
            public void EPItestipluskal_pcr_20160416()
            {
                this.EpiTestCliBase(@"D:\pcaps\pcpluskal2\pcr-20160416.cap", 0.9);
            }

            [Test]
            public void MLtestipluskal_pcr_20160416()
            {
                this.MLTestCliBase(@"D:\pcaps\pcpluskal2\pcr-20160416.cap", 0.9);
            }

            private void CompareStatistics(ApplicationProtocolClassificationStatisticsMeter newStatistics, string fileNameOldStatsXml)
            {
                var oldStatistics = this.AppIdentService.LoadStatisticsFromXml(fileNameOldStatsXml);


                int newScore = 0;
                int oldScore = 0;
                bool change = false;

                Console.WriteLine("##############Compare statistics##############");
                foreach(var applicationStatistic in newStatistics.AppStatistics)
                {
                    if(!oldStatistics.AppStatistics.ContainsKey(applicationStatistic.Key))
                    {
                        Console.WriteLine("Missing application " + applicationStatistic.Key + " in old statistics.\n");
                        continue;
                    }

                    var oldStat = oldStatistics.AppStatistics[applicationStatistic.Key];
                    var newPrec = applicationStatistic.Value.Precission;
                    var oldPrec = oldStat.Precission;
                    if(newPrec.CompareTo(oldPrec) > 0)
                    {
                        newScore++;
                        change = true;
                    }
                    else if(newPrec.CompareTo(oldPrec) < 0)
                    {
                        oldScore++;
                        change = true;
                    }

                    var newRec = applicationStatistic.Value.Recall;
                    var oldRec = oldStat.Recall;
                    if(newRec.CompareTo(oldRec) > 0)
                    {
                        newScore++;
                        change = true;
                    }
                    else if(newRec.CompareTo(oldRec) < 0)
                    {
                        oldScore++;
                        change = true;
                    }

                    var newFmes = applicationStatistic.Value.FMeasure;
                    var oldFmes = oldStat.FMeasure;
                    if(newFmes.CompareTo(oldFmes) > 0)
                    {
                        newScore++;
                        change = true;
                    }
                    else if(newFmes.CompareTo(oldFmes) < 0)
                    {
                        oldScore++;
                        change = true;
                    }

                    if(change)
                    {
                        Console.WriteLine(applicationStatistic.Key);
                        Console.WriteLine("New model TP: " + applicationStatistic.Value.TP + " FP: " + applicationStatistic.Value.FP + " FN: " + applicationStatistic.Value.FN);
                        Console.WriteLine("Old model TP: " + oldStat.TP + " FP: " + oldStat.FP + " FN: " + oldStat.FN);
                        Console.WriteLine("New model/Old model Precision: " + newPrec + "/" + oldPrec);
                        Console.WriteLine("New model/Old model Recall: " + newRec + "/" + oldRec);
                        Console.WriteLine("New model/Old model F-Measure: " + newFmes + "/" + oldFmes);
                        Console.WriteLine();
                    }
                    change = false;
                }

                Console.WriteLine("Score new/old: " + newScore + "/" + oldScore);
            }


        }
    }


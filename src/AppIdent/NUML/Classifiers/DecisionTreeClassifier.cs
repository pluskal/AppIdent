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



using System.Collections.Generic;
using AppIdent.Models;
using numl;
using numl.Model;
using numl.Supervised.DecisionTree;

namespace AppIdent.NUML.Classifiers
{
    public class DecisionTreeClassifier : ClassifierBase
    {
        public DecisionTreeClassifier(FeatureVector[] stats) : base(stats) { }

        public override void CreateClassifier()
        {
            // this.Normalizator = new Normalizator.Normalizator(this.Stats);
            // this.Stats = this.Normalizator.Normalize(this.Stats);
            var testSet = this.Stats;
            var descriptor = Descriptor.Create<FeatureVector>();
            var decisionTreeModel = new DecisionTreeGenerator(descriptor.VectorLength, 2, descriptor, null, 0.0);
            // decisionTreeModel.SetHint(false);
            decisionTreeModel.Generate(testSet);
            var learnDecisionTree = Learner.Learn(testSet, 0.90, 100, decisionTreeModel);
            this.ClassifierModel = learnDecisionTree.Model;
        }
    }
}
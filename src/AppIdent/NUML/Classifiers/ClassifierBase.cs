﻿//Copyright (c) 2017 Jan Pluskal
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
using AppIdent.Normalization;
using numl.Supervised;

namespace AppIdent.NUML.Classifiers
{
    public abstract class ClassifierBase
    {
        protected FeatureVector[] Stats;

        protected ClassifierBase(FeatureVector[] stats)
        {
            this.Stats = stats;
            this.Normalizator = new Normalizator();
            this.Normalizator.Initialize(this.Stats);
            this.Normalizator.Normalize(this.Stats);
            this.CreateClassifier();
        }

        public Normalizator Normalizator { get; }

        public IModel ClassifierModel { get; internal set; }

        public abstract void CreateClassifier();
    }
}
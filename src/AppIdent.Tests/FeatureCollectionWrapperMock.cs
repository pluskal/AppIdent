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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AppIdent.Features.Bases;
using AppIdent.Misc;
using AppIdent.Models;

namespace AppIdent.Tests.Features.Bases
{
    public class FeatureCollectionWrapperMock<TFeature> : IFeatureCollectionWrapper<TFeature> where TFeature : FeatureBase
    {
        public TFeature[] Features { get; }

        public FeatureCollectionWrapperMock(IEnumerable<TFeature> features) {
            this.Features = features as TFeature[] ?? features.ToArray();
        }
        public TFeature this[int i] => this.Features[i];

        public FeatureVector[] FeatureVectors {get { throw new NotImplementedException(); } }

        public PropertyInfo FeaturePropertyInfo { get { throw new NotImplementedException(); } }

        public IEnumerator<TFeature> GetEnumerator() => ((IEnumerable<TFeature>) this.Features).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.Features.GetEnumerator();
    }
}
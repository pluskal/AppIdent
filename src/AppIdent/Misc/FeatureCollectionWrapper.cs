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
using AppIdent.Models;

namespace AppIdent.Misc
{
    public class FeatureCollectionWrapper<T> : IFeatureCollectionWrapper<T> where T : FeatureBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        public FeatureCollectionWrapper(FeatureVector[] featureVectors)
        {
            this.FeatureVectors = featureVectors;
            var props = typeof(FeatureVector).GetProperties().Where(prop => prop.PropertyType == typeof(T));
            if(props.Count() != 1)
            {
                throw new InvalidOperationException("IdentificationFeatures cannot contain two properties of the same type - {props.FirstOrDefault()?.PropertyType}");
            }
            this.FeaturePropertyInfo = props.First();
        }

        public PropertyInfo FeaturePropertyInfo { get; }
        public FeatureVector[] FeatureVectors { get; }

        public T this[int i] => (T) this.FeaturePropertyInfo.GetValue(this.FeatureVectors[i], null);

        #region Implementation of IEnumerable
        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this.FeatureVectors.Select(feature => (T) this.FeaturePropertyInfo.GetValue(feature, null)).GetEnumerator();
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion
    }
}
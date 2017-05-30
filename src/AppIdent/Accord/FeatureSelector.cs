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
using AppIdent.Features.Bases;
using AppIdent.Models;

namespace AppIdent.Accord {
    [Serializable]
    public class FeatureSelector
    {
        public IReadOnlyList<Type> SelectedFeatures => this._selectedFeatures.AsReadOnly();
        private readonly List<Type> _selectedFeatures;
        public FeatureSelector(IEnumerable<Type> selectedFeatureTypes)
        {
            this._selectedFeatures = new List<Type>(selectedFeatureTypes);
        }
        public FeatureSelector()
        {
            var selectedFeatureTypes = typeof(FeatureVector).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(FeatureStatisticalAttribute)))
                .Select(p => p.PropertyType);
           this._selectedFeatures = new List<Type>(selectedFeatureTypes);
        }

        public FeatureSelector(IEnumerable<Type> selectedFeatureTypes, bool skipDiscrete)
        {
            if(!skipDiscrete)
            {
                this._selectedFeatures = new List<Type>(selectedFeatureTypes);
                return;
            }

            this._selectedFeatures = new List<Type>();
            foreach (var selectedFeatureType in selectedFeatureTypes)
            {
                var feature = Activator.CreateInstance(selectedFeatureType) as FeatureBase;
                 if(feature.FeatureKind == FeatureKind.Discrete) continue;
                this._selectedFeatures.Add(selectedFeatureType);
            }
        }

        public void RemoveFeature(Type feature) => this._selectedFeatures.Remove(feature);
    }
}
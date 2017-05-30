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

namespace AppIdent.Misc
{
    public sealed class ArrayEqualityComparer<T> : IEqualityComparer<T[]>
    {
        // You could make this a per-instance field with a constructor parameter
        private static readonly EqualityComparer<T> elementComparer = EqualityComparer<T>.Default;

        public bool Equals(T[] first, T[] second)
        {
            if(first == second) { return true; }
            if(first == null || second == null) { return false; }
            if(first.Length != second.Length) { return false; }
            for(var i = 0; i < first.Length; i++) { if(!elementComparer.Equals(first[i], second[i])) { return false; } }
            return true;
        }

        public int GetHashCode(T[] array)
        {
            unchecked
            {
                if(array == null) { return 0; }
                var hash = 17;
                foreach(var element in array) { hash = hash * 31 + elementComparer.GetHashCode(element); }
                return hash;
            }
        }
    }
}
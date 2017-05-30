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

namespace AppIdent.Misc
{
    public class Entropy
    {
        public static double Calculate<T>(IEnumerable<EntropyItem<T>> items)
        {
            var classes = 0;
            return Calculate(items, out classes);
        }

        public static double Calculate<T>(IEnumerable<EntropyItem<T>> items, out int classes)
        {
            var enthropyItems = items as EntropyItem<T>[] ?? items.ToArray();
            classes = enthropyItems.Sum(item => item.Count);
            var enth = 0.0;
            foreach(var item in enthropyItems)
            {
                var px = item.Count / (double) classes;
                enth -= (px * Math.Log(px));
            }
            return enth;
        }

        public static double Calculate<T>(IEnumerable<T> items)
        {
            var classes = 0;
            return Calculate(items, out classes);
        }

        public static double Calculate<T>(IEnumerable<T> items, out int classes)
        {
            var histogram = items.GroupBy(item => item).Select(g => new EntropyItem<T>
            {
                Key = g.Key,
                Count = g.Count()
            });
            return Calculate(histogram, out classes);
        }

        public class EntropyItem<T>
        {
            public T Key { get; set; }
            public int Count { get; set; }
        }
    }
}
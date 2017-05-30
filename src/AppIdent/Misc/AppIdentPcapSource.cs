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

namespace AppIdent.Misc {
    [Serializable]
    public class AppIdentPcapSource
    {
        public IReadOnlyList<string> TestingPcaps => this._testingPcaps;
        public IReadOnlyList<string> VerificationPcaps => this._verificationPcaps;
        private readonly List<string> _testingPcaps  = new List<string>();
        private readonly List<string> _verificationPcaps = new List<string>();

        public void AddTesting(string pcapFilePath)
        {
            this._testingPcaps.Add(pcapFilePath);
        }
        public void AddVerification(string pcapFilePath)
        {
            this._verificationPcaps.Add(pcapFilePath);
        }

        public void AddTesting(string directoryFilePath, string wildcard, bool recursive)
        {
            var extensions = wildcard.Split('|');
            foreach (var extension in extensions)
            {
                var pcapFilePaths = Directory.GetFiles(directoryFilePath, extension, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                foreach (var pcapFilePath in pcapFilePaths)
                {
                    this._testingPcaps.Add(pcapFilePath);
                }
            }
        }
        public void AddVerification(string directoryFilePath, string wildcard, bool recursive)
        {
            var extensions = wildcard.Split('|');
            foreach(var extension in extensions)
            {
                var pcapFilePaths = Directory.GetFiles(directoryFilePath, extension, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                foreach (var pcapFilePath in pcapFilePaths)
                {
                    this._verificationPcaps.Add(pcapFilePath);
                }
            }
        }
    }
}
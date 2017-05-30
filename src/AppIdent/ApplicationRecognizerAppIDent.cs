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



/*
 *
 *
 */

using System;
using System.Collections.Generic;
using Framework.Models;
using Framework.Models.Services;
using NBARDatabase;

namespace AppIdent
{
    //Todo IMPLEMENT
    public class ApplicationRecognizerAppIDent : ApplicationRecognizerBase
    {
        public ApplicationRecognizerAppIDent(NBARProtocolPortDatabase NBARProtocolPortDatabase) : base(NBARProtocolPortDatabase) { }

        public override String Name => @"AppIdent";

        public override String Description => @"Application identification using ML.";

        public override UInt32 Priority => 4;

        public override String Type => "ML, flow stats.";

        public override IReadOnlyList<NBAR2TaxonomyProtocol> RecognizeConversation(L7Conversation conversation)
        {
            //var appTag = this.ModelExtractor.RunRecognition(conversation);
            //if (appTag == null) { return new List<NBAR2TaxonomyProtocol>(); }
            //return new List<NBAR2TaxonomyProtocol>
            //{
            //    this.NBARProtocolPortDatabase.GetNbar2TaxonomyProtocol(appTag)
            //};
            return null;
        }
    }
}
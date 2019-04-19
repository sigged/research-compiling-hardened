using Sigged.CsC.CodeSamples.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sigged.CsC.NetCore.Web.Models
{
    public class CodeSampleCategory
    {
        public string Category { get; set; }
        public IEnumerable<CodeSample> Samples { get; set; }
    }
}

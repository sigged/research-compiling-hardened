using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sigged.CsCNetFx.Wpf
{
    public class DiagnosticViewModel
    {
        public DiagnosticViewModel(Diagnostic diagnostic)
        {
            Diagnostic = diagnostic;
        }

        public Diagnostic Diagnostic { get; set; }

        public string Message => Diagnostic?.GetMessage();

        public string Location
        {
            get
            {
                string result = "";
                var pos = Diagnostic.Location.GetLineSpan();
                if (pos.Path != null)
                {
                    // user-visible line and column counts are 1-based, but internally are 0-based.
                    result += (pos.StartLinePosition.Line + 1) + ":" + (pos.StartLinePosition.Character + 1);
                }

                return result;
            }
        }
    }
}

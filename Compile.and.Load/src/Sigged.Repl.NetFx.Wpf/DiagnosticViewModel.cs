using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sigged.Repl.NetFx.Wpf
{
    public class DiagnosticViewModel
    {
        public DiagnosticViewModel(Diagnostic diagnostic)
        {
            Diagnostic = diagnostic;
        }

        public Diagnostic Diagnostic { get; set; }

        public string Message => Diagnostic?.GetMessage();
    }
}

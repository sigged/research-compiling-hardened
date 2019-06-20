using System;
using System.IO;
using System.Text;

namespace Sigged.CsC.NetFx.Wpf
{
    public class CrossDomainConsoleOutputWriter : TextWriter
    {
        public event ConsoleOutputHandler OnConsoleOutput;
        
        public CrossDomainConsoleOutputWriter()
        {
        }

        public override void Write(char value)
        {
            OnConsoleOutput?.Invoke(this, value.ToString());
        }

        public override void Write(string value)
        {
            OnConsoleOutput?.Invoke(this, value);
        }

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }
    }
}

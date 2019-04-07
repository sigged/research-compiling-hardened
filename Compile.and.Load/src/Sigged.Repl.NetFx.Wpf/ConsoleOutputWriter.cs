using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Sigged.Repl.NetFx.Wpf
{
    public class ConsoleOutputWriter : TextWriter
    {
        // The control where we will write text.
        private TextBlock outputControl;
        public ConsoleOutputWriter(TextBlock outputControl)
        {
            this.outputControl = outputControl;
        }

        public override void Write(char value)
        {
            outputControl.Dispatcher.Invoke(() =>
            {
                outputControl.Text += value;
            });
        }

        public override void Write(string value)
        {
            outputControl.Dispatcher.Invoke(() =>
            {
                outputControl.Text += value;
            });
        }

        public override Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }
    }

    //public class ConsoleRedirector : IDisposable
    //{
    //    //private TextWriter originalOut = Console.Out;
    //    //private AnonymousPipeServerStream consoleOutServerPipe;
    //    //private StreamWriter currentOut;

    //    //public ConsoleRedirector()
    //    //{
    //    //    this.consoleOutServerPipe = new AnonymousPipeServerStream(PipeDirection.Out);
    //    //    this.currentOut = new StreamWriter(this.consoleOutServerPipe);
    //    //    this.currentOut.AutoFlush = true;
    //    //    Console.SetOut(this.currentOut);
    //    //    ThreadPool.QueueUserWorkItem(o => { this.Listen(); });
    //    //}

    //    //private void Listen()
    //    //{
    //    //    AnonymousPipeClientStream consoleOutClientPipe = new AnonymousPipeClientStream(PipeDirection.In, this.consoleOutServerPipe.ClientSafePipeHandle);
    //    //    using (StreamReader fileIn = new StreamReader(consoleOutClientPipe))
    //    //    {
    //    //        int text = fileIn.Read();
    //    //    }
    //    //}

    //    //public void Dispose()
    //    //{
    //    //    this.currentOut.Dispose();
    //    //    Console.SetOut(this.originalOut);
    //    //}


    //}
}

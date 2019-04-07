using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Sigged.Repl.NetFx.Wpf
{

    public class ConsoleInputReader : TextReader
    {

        // The control where we will write text.
        private TextBox inputControl;
        private TextBlock outputControl;
        public ConsoleInputReader(TextBox inputControl, TextBlock outputControl)
        {
            this.inputControl = inputControl;
            this.outputControl = outputControl;
        }

        public override int Read()
        {
            string original = "";
            string input = "";
            inputControl.Dispatcher.Invoke(() =>
            {
                original = inputControl.Text;
                inputControl.IsEnabled = true;
                inputControl.Focus();
            });
            while (original == input)
            {
                inputControl.Dispatcher.Invoke(() =>
                {
                    input = inputControl.Text;
                });
                Thread.Sleep(100);
            }
            string sub = original.Length == 0 ? input : input.Replace(original, "");
            inputControl.Dispatcher.Invoke(() =>
            {
                outputControl.Text += inputControl.Text;
                inputControl.Clear();
                inputControl.IsEnabled = false;
            });
            return sub[sub.Length - 1];
        }

        public override string ReadLine()
        {
            inputControl.Dispatcher.Invoke(() =>
            {
                inputControl.IsEnabled = true;
                inputControl.Focus();
            });

            string input = "";
            while (!input.EndsWith(Environment.NewLine))
            {
                inputControl.Dispatcher.Invoke(() =>
                {
                    input = inputControl.Text;
                    
                });
                Thread.Sleep(100);
            }

            inputControl.Dispatcher.Invoke(() =>
            {
                outputControl.Text += inputControl.Text;
                inputControl.Clear();
                inputControl.IsEnabled = false;
            });

            return input;
        }

        public override int Read(char[] buffer, int index, int count)
        {
            buffer = new char[count];
            return 0;
        }
    }
}

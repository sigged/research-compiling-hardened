using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sigged.Repl.NetFx.Wpf
{

    public class ConsoleInputReader : TextReader
    {
        private TextBox consoleBox;
        private Stack<Key> inputKeyQueue;
        private Stack<char> inputCharQueue;

        private bool disableSelectionEvent = false;
        private bool disableInput;

        public bool DisableInput
        {
            get { return disableInput; }
            set {
                disableInput = value;
                consoleBox.Dispatcher.Invoke(() =>
                {
                    consoleBox.IsReadOnly = disableInput;
                });
            }
        }
        
        public ConsoleInputReader(TextBox consolebox)
        {
            consoleBox = consolebox;
            inputKeyQueue = new Stack<Key>();
            inputCharQueue = new Stack<char>();

            DisableInput = true;

            consoleBox.PreviewTextInput += ConsoleBox_PreviewTextInput;
            consoleBox.PreviewKeyDown += ConsoleBox_PreviewKeyDown;
            consoleBox.SelectionChanged += ConsoleBox_SelectionChanged;
        }

        private void ConsoleBox_SelectionChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!DisableInput && !disableSelectionEvent)
            {
                consoleBox.Dispatcher.Invoke(() =>
                {
                    disableSelectionEvent = true;
                    consoleBox.CaretIndex = consoleBox.Text?.Length ?? 0;
                    disableSelectionEvent = false;
                });
            }
        }

        private void ConsoleBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
                inputCharQueue.Push(c);
        }

        private void ConsoleBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Back)
            {
                inputKeyQueue.Pop();
                inputCharQueue.Pop();

                if (!DisableInput && inputCharQueue.Count == 0)
                    e.Handled = true;
            }
            else
            {
                inputKeyQueue.Push(e.Key);
            }
            
        }

        public override int Read()
        {
            try
            {
                inputCharQueue.Clear();
                inputKeyQueue.Clear();
                DisableInput = false;

                consoleBox.Dispatcher.Invoke(() =>
                {
                    consoleBox.CaretIndex = consoleBox.Text?.Length ?? 0;
                    consoleBox.Focus();
                });

                while (inputCharQueue.Count == 0)
                {
                    Thread.Sleep(10);
                }

                return inputCharQueue.Pop();
            }
            finally
            {
                DisableInput = true;
            }
            
        }

        public override string ReadLine()
        {
            try
            {
                inputCharQueue.Clear();
                inputKeyQueue.Clear();
                DisableInput = false;

                consoleBox.Dispatcher.Invoke(() =>
                {
                    consoleBox.CaretIndex = consoleBox.Text?.Length ?? 0;
                    consoleBox.Focus();
                });

                while (!inputCharQueue.Contains('\r'))
                {
                    Thread.Sleep(10);
                }

                consoleBox.Dispatcher.Invoke(() =>
                {
                    consoleBox.Text += "\n";
                });
                return new string(inputCharQueue.Reverse().ToArray());
            }
            finally
            {
                inputCharQueue.Clear();
                inputKeyQueue.Clear();
                DisableInput = true;
            }
        }

        public override int Read(char[] buffer, int index, int count)
        {
            buffer = new char[count];
            return 0;
        }
    }

    public class ConsoleInputReaderOld : TextReader
    {

        // The control where we will write text.
        private TextBox inputControl;
        private TextBlock outputControl;
        public ConsoleInputReaderOld(TextBox inputControl, TextBlock outputControl)
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

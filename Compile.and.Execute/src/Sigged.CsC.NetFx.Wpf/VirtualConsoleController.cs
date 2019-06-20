using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sigged.CsC.NetFx.Wpf
{
    public class VirtualConsoleController
    {
        private TextBox consoleBox;
        InputAggregator inputAggregator;

        private bool disableSelectionEvent = false;
        private bool disableInput;

        public bool DisableInput
        {
            get { return disableInput; }
            set
            {
                disableInput = value;
                consoleBox.Dispatcher.Invoke(() =>
                {
                    consoleBox.IsReadOnly = disableInput;
                });
            }
        }

        public VirtualConsoleController(TextBox consolebox, InputAggregator inputaggregator)
        {
            consoleBox = consolebox;

            inputAggregator = inputaggregator;
            inputaggregator.WaitForUserInput += Inputaggregator_WaitForUserInput;
            inputaggregator.EndWaitForUserInput += Inputaggregator_EndWaitForUserInput;

            DisableInput = true;

            consoleBox.PreviewTextInput += ConsoleBox_PreviewTextInput;
            consoleBox.PreviewKeyDown += ConsoleBox_PreviewKeyDown;
            consoleBox.SelectionChanged += ConsoleBox_SelectionChanged;
        }

        private void Inputaggregator_WaitForUserInput(object sender, EventArgs e)
        {

            DisableInput = false;

            consoleBox.Dispatcher.Invoke(() =>
            {
                consoleBox.CaretIndex = consoleBox.Text?.Length ?? 0;
                consoleBox.Focus();
            });
        }

        private void Inputaggregator_EndWaitForUserInput(object sender, EventArgs e)
        {
            DisableInput = true;
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
                inputAggregator.InputCharQueue.Push(c);
        }

        private void ConsoleBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                inputAggregator.InputKeyQueue.Pop();
                inputAggregator.InputCharQueue.Pop();

                if (!DisableInput && inputAggregator.InputCharQueue.Count == 0)
                    e.Handled = true;
            }
            else
            {
                inputAggregator.InputKeyQueue.Push(e.Key);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Sigged.CsC.NetFx.Wpf
{
    [Serializable]
    public class InputAggregator : MarshalByRefObject
    {
        public event EventHandler WaitForUserInput;
        public event EventHandler EndWaitForUserInput;

        public InputAggregator()
        {
            InputKeyQueue = new Stack<Key>();
            InputCharQueue = new Stack<char>();
        }

        public Stack<Key> InputKeyQueue { get; set; }
        public Stack<char> InputCharQueue { get; set; }

        public int GetUserInputChar()
        {
            try
            {
                InputCharQueue.Clear();
                InputCharQueue.Clear();
                WaitForUserInput?.Invoke(this, EventArgs.Empty);

                while (InputCharQueue.Count == 0)
                {
                    Thread.Sleep(10);
                }

                return InputCharQueue.Pop();
            }
            finally
            {
                EndWaitForUserInput?.Invoke(this, EventArgs.Empty);
            }
        }

        public string GetUserInputString()
        {
            try
            {
                InputCharQueue.Clear();
                InputCharQueue.Clear();

                WaitForUserInput?.Invoke(this, EventArgs.Empty);

                while (!InputCharQueue.Contains('\r'))
                {
                    Thread.Sleep(10);
                }
                
                InputCharQueue.Pop(); //pops \r

                return new string(InputCharQueue.Reverse().ToArray());
            }
            finally
            {
                EndWaitForUserInput?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}

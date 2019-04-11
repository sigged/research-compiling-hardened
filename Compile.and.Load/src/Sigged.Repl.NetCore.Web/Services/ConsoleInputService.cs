using System;
using System.IO;
using System.Threading;

namespace Sigged.Repl.NetCore.Web.Services
{
    public class ConsoleInputService : TextReader
    {
        protected IRemoteExecutionCallback executionCallback;
        protected RemoteCodeSession session;

        public event EventHandler RemoteInputReceived;

        public string receivedInput = null;

        public ConsoleInputService(RemoteCodeSession session, IRemoteExecutionCallback executionCallback)
        {
            this.session = session;
            this.executionCallback = executionCallback;
        }

        public override int Read()
        {
            try
            {
                executionCallback.SendExecutionStateChanged(session, new RemoteExecutionState
                {
                    State = RemoteAppState.WaitForInput
                });
                while (receivedInput == null)
                {
                    Thread.Sleep(100);
                }
                string input = receivedInput;
                return input[0];
            }
            finally
            {
                receivedInput = null;
            }
        }

        public override string ReadLine()
        {
            try
            {
                executionCallback.SendExecutionStateChanged(session, new RemoteExecutionState
                {
                    State = RemoteAppState.WaitForInputLine
                });
                while(receivedInput == null)
                {
                    Thread.Sleep(100);
                }
                string input = receivedInput;
                return input;
            }
            finally
            {
                receivedInput = null;
            }
        }

        public override int Read(char[] buffer, int index, int count)
        {
            buffer = new char[count];
            return 0;
        }


        public void ReceiveInput(string input)
        {
            receivedInput = input;
        }

    }
}

using System.IO;
using System.Text;
using System.Web;

namespace Sigged.Repl.NetCore.Web.Services
{
    public class ConsoleOutputService : TextWriter
    {
        protected IRemoteExecutionCallback executionCallback;
        protected RemoteCodeSession session;

        public ConsoleOutputService(RemoteCodeSession session, IRemoteExecutionCallback executionCallback)
        {
            this.session = session;
            this.executionCallback = executionCallback;
        }

        public override void Write(char value)
        {
            executionCallback.SendExecutionStateChanged(session, new RemoteExecutionState
            {
                State = RemoteAppState.WriteOutput,
                Output = HttpUtility.HtmlEncode(value.ToString())
            });
        }

        public override void Write(string value)
        {
            executionCallback.SendExecutionStateChanged(session, new RemoteExecutionState
            {
                State = RemoteAppState.WriteOutput,
                Output = HttpUtility.HtmlEncode(value.ToString())
            });
        }

        public override Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

    }
}

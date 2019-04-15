using ProtoBuf;
using Sigged.CodeHost.Core.Dto;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Web;

namespace Sigged.CodeHost.Worker
{
    public class ConsoleOutputService : TextWriter
    {
        protected string sessionid;
        protected TcpClient client;
        protected Stream networkStream;

        public ConsoleOutputService(string sessionid, TcpClient client)
        {
            this.sessionid = sessionid;
            this.client = client;
            this.networkStream = client.GetStream();
        }

        public override void Write(char value)
        {
            var execState = new ExecutionStateDto
            {
                SessionId = sessionid,
                State = RemoteAppState.WriteOutput,
                Output = HttpUtility.HtmlEncode(value.ToString())
            };
            networkStream.WriteByte((byte)MessageType.WorkerExecutionState);
            Serializer.SerializeWithLengthPrefix(networkStream, execState, PrefixStyle.Fixed32);
        }

        public override void Write(string value)
        {
            var execState = new ExecutionStateDto
            {
                SessionId = sessionid,
                State = RemoteAppState.WriteOutput,
                Output = HttpUtility.HtmlEncode(value.ToString())
            };
            networkStream.WriteByte((byte)MessageType.WorkerExecutionState);
            Serializer.SerializeWithLengthPrefix(networkStream, execState, PrefixStyle.Fixed32);
        }

        public override Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

    }
}

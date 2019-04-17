using System.Net.Sockets;
using System.Threading.Tasks;

namespace Sigged.CodeHost.Core.Worker
{
    public interface IWorker
    {
        Task Start(string host, int port, string sessionid);
        void Stop();
        void RunApplication(string sessionid, TcpClient client, byte[] assemblyBytes);
    }
}

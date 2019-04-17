using Sigged.CodeHost.Core.Worker;

namespace Sigged.CodeHost.Worker
{
    class Program
    {
        static IWorker worker = new Worker();

        static void Main(string[] args) {

            //gather arguments
            string host = args[0];
            int port = int.Parse(args[1]);
            string sessionid = args[2];

            //begin work
            worker.Start(host, port, sessionid).Wait();
        }
    }
}

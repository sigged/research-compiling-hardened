
using Sigged.CodeHost.Core.Dto;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;

namespace Sigged.Compling.Core.CodeHost
{
    /// <summary>
    /// Sample Server app for codehost client
    /// </summary>
    class Program
    {
        protected const string PIPENAME = "codehost21.pipe";

        protected const string SESSIONID = "the-unique-session-id";

        static BuilderTcpListener listener;

        static void Main(string[] args)
        {
            listener = new BuilderTcpListener("0.0.0.0", 2000, "BuilderListener");
            listener.Connect();

            while(true)
            {
                Task.Delay(100).Wait();
            }

        }
    }
}

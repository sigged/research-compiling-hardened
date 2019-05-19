using Sigged.CodeHost.Core.Logging;
using Sigged.CsC.NetCore.Web.Constants;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Sigged.CsC.NetCore.Web.Models
{
    public class DockerWorkerProcess : IWorkerProcess
    {
        protected string _containerName;
        protected const string CONTAINER_PREFIX = "worker";

        public DockerWorkerProcess()
        {
        }

        public void Start(string host, int port, string sessionid)
        {
            if(!string.IsNullOrWhiteSpace(sessionid))
            {
                _containerName = $"{CONTAINER_PREFIX}_{sessionid}";
                using (var process = new Process())
                {
                    Kill();
                    Logger.LogLine($"Worker Control: (docker) starting container {_containerName}");
                    process.StartInfo = CreateDefaultStartInfo("docker", $"run --detach --rm --name {_containerName} --link insecure-csc-hardened:{host} sigged/insecure-csc-worker {host} {port} {sessionid}");
                    process.Start();
                }
            }
        }

        public bool HasExited()
        {
            Logger.LogLine($"Worker Control: (docker) checking state of container {_containerName}");
            string outputData = "", errorData = "";
            using (var process = new Process())
            {
                process.StartInfo = CreateDefaultStartInfo("docker", $"inspect -f {{.State.Running}} {_containerName}");
                process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => {
                    outputData += e.Data;
                };
                process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => {
                    errorData += e.Data;
                };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit(SessionConstants.DockerCommandIdleTimeout * 1000);
                if (!process.HasExited)
                    process.Kill();

                process.Close();

                //Console.WriteLine("HasExited STDOUT follows:");
                //Console.Write(outputData);
                //Console.WriteLine("HasExited STDERR follows:");
                //Console.Write(errorData);
                return outputData.StartsWith("true");
            }
        }

        public void Kill()
        {
            Logger.LogLine($"Worker Control: (docker) killing container {_containerName}");
            using (var process = new Process())
            {
                process.StartInfo = CreateDefaultStartInfo("docker", $"rm -f {_containerName}");
                process.Start();
                process.WaitForExit(SessionConstants.DockerCommandIdleTimeout * 1000);
                if (!process.HasExited)
                    process.Kill();
                process.Close();
            }
        }

        public void Dispose()
        {
            Kill();
        }

        private ProcessStartInfo CreateDefaultStartInfo(string command, string arguments)
        {
            var processInfo = new ProcessStartInfo(command, arguments);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;
            return processInfo;
        }
    }
}

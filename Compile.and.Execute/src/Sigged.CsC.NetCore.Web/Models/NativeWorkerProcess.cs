using System.Diagnostics;

namespace Sigged.CsC.NetCore.Web.Models
{

    public class NativeWorkerProcess : IWorkerProcess
    {
        protected Process _processRef;
        protected string _workerExePath;

        public NativeWorkerProcess(string workerExePath)
        {
            _workerExePath = workerExePath;
        }

        public void Start(string host, int port, string sessionid)
        {
            _processRef = new Process();
            _processRef.StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"{_workerExePath} {host} {port} {sessionid}",
                UseShellExecute = false
            };

            _processRef.Start();
        }

        public bool HasExited()
        {
            return _processRef?.HasExited == true;
        }

        public void Kill()
        {
            _processRef?.Kill();
        }

        public void Dispose()
        {
            if (!HasExited())
            {
                _processRef?.Kill();
            }
            _processRef?.Dispose();
        }

    }
}

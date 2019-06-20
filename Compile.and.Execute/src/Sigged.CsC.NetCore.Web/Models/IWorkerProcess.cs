using System;

namespace Sigged.CsC.NetCore.Web.Models
{
    public interface IWorkerProcess : IDisposable
    {
        void Start(string host, int port, string sessionid);
        void Kill();
        bool HasExited();
    }

    

}

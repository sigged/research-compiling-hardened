using Sigged.CodeHost.Core.Dto;
using System.Threading.Tasks;

namespace Sigged.CsC.NetCore.Web.Services
{
    public interface IClientService
    {
        Task Connect();
        Task SendBuildResult(string sessionId, BuildResultDto results);
        Task SendExecutionState(string sessionId, ExecutionStateDto state);
    }
}

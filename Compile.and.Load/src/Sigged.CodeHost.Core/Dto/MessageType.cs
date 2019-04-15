using System;
using System.Collections.Generic;
using System.Text;

namespace Sigged.CodeHost.Core.Dto
{
    public enum MessageType : byte
    {
        WorkerIdentification = 1,
        ServerBuildRequest = 2,
        ServerRemoteInput = 3,
        WorkerBuildResult = 4,
        WorkerExecutionState = 5,
    }
}

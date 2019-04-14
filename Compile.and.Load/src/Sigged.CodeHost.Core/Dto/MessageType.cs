using System;
using System.Collections.Generic;
using System.Text;

namespace Sigged.CodeHost.Core.Dto
{
    public enum MessageType : byte
    {
        ServerBuildRequest = 1,
        ClientBuildResult = 2,
        ClientExectionState = 3,
        ServerRemoteInput = 4
    }
}

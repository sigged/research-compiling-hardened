using System;
using System.Collections.Generic;
using System.Text;

namespace Sigged.CodeHost.Core.Dto
{
    public enum MessageType : byte
    {
        ClientIdentification = 1,
        ServerBuildRequest = 2,
        ServerRemoteInput = 3,
        ClientBuildResult = 4,
        ClientExectionState = 5,
    }
}

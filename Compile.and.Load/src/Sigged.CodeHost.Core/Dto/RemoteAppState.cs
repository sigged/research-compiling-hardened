using ProtoBuf;
using System;

namespace Sigged.CodeHost.Core.Dto
{
    [ProtoContract]
    [Serializable]
    public enum RemoteAppState : byte
    {
        NotRunning = 0,
        Running = 1,
        WriteOutput = 10,
        WaitForInput = 11,
        WaitForInputLine = 12,
        Crashed = 20,
        Ended = 100
    }
}

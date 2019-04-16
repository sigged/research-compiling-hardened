using ProtoBuf;
using System;

namespace Sigged.CodeHost.Core.Dto
{
    [ProtoContract]
    [Serializable]
    public class ExecutionStateDto
    {
        [ProtoMember(1)]
        public string SessionId { get; set; }
        [ProtoMember(2)]
        public RemoteAppState State { get; set; }
        [ProtoMember(3)]
        public ExceptionDto Exception { get; set; }
        [ProtoMember(4)]
        public string Output { get; set; }
    }
}

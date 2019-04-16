using ProtoBuf;
using System;

namespace Sigged.CodeHost.Core.Dto
{
    [ProtoContract]
    [Serializable]
    public class BuildRequestDto
    {
        [ProtoMember(1)]
        public string SessionId { get; set; }
        [ProtoMember(2)]
        public string SourceCode { get; set; }
        [ProtoMember(3)]
        public bool RunOnSuccess { get; set; }
    }
}

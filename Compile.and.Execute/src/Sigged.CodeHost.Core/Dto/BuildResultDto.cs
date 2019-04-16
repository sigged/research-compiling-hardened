using ProtoBuf;
using System;
using System.Collections.Generic;

namespace Sigged.CodeHost.Core.Dto
{
    [ProtoContract]
    [Serializable]
    public class BuildResultDto
    {
        [ProtoMember(1)]
        public string SessionId { get; set; }
        [ProtoMember(2)]
        public bool IsSuccess { get; set; }
        [ProtoMember(3)]
        public List<BuildErrorDto> BuildErrors { get; set; }
    }
}

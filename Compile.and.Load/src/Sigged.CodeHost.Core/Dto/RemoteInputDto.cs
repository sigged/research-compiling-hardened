using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sigged.CodeHost.Core.Dto
{
    [ProtoContract]
    [Serializable]
    public class RemoteInputDto
    {
        [ProtoMember(1)]
        public string SessionId { get; set; }
        [ProtoMember(2)]
        public string Input { get; set; }
    }
}

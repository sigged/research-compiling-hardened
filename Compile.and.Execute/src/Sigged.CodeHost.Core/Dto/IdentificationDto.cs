using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sigged.CodeHost.Core.Dto
{
    [ProtoContract]
    [Serializable]
    public class IdentificationDto
    {
        [ProtoMember(1)]
        public string SessionId { get; set; }
    }
}

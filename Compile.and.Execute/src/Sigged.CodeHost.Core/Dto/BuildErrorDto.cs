using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sigged.CodeHost.Core.Dto
{
    [ProtoContract]
    [Serializable]
    public class BuildErrorDto
    {
        [ProtoMember(1)]
        public string Severity { get; set; }

        [ProtoMember(2)]
        public string Id { get; set; }

        [ProtoMember(3)]
        public LinePositionDto StartPosition { get; set; }

        [ProtoMember(4)]
        public LinePositionDto EndPosition { get; set; }

        [ProtoMember(5)]
        public string Description { get; set; }
    }
}

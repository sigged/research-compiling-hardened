using Microsoft.CodeAnalysis.Text;
using ProtoBuf;
using System;

namespace Sigged.CodeHost.Core.Dto
{
    [ProtoContract]
    [Serializable]
    public class LinePositionDto
    {
        /// <summary>
        /// The line number. The first line in a file is defined as line 0 (zero based line numbering).
        /// </summary>
        [ProtoMember(1)]
        public int Line { get; set; }

        /// <summary>
        /// The character position within the line.
        /// </summary>
        [ProtoMember(2)]
        public int Character { get; set; }

        public static LinePositionDto FromLinePosition(LinePosition linePos)
        {
            return new LinePositionDto
            {
                Line = linePos.Line,
                Character = linePos.Character
            };
        }
    }
}

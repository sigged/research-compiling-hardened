using ProtoBuf;
using Sigged.CodeHost.Core.Extensions;
using System;
using System.Diagnostics;

namespace Sigged.CodeHost.Core.Dto
{
    [ProtoContract]
    [Serializable]
    public class ExceptionDto
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Message { get; set; }

        public static ExceptionDto FromException(Exception exception)
        {
            exception = exception.GetInnermostException();
            if (exception != null)
            {
                int? linenumber = (new StackTrace(exception, true))?.GetFrame(0)?.GetFileLineNumber();

                return new ExceptionDto
                {
                    Name = exception.GetType().Name,
                    Message = exception.Message
                };
            }
            return null;
        }
    }
}

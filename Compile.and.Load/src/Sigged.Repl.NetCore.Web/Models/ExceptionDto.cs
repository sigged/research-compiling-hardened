using Sigged.Repl.NetCore.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Sigged.Repl.NetCore.Web.Models
{
    public class ExceptionDescriptor
    {
        protected ExceptionDescriptor()
        {
        }

        public string Name { get; set; }
        public string Message { get; set; }

        public static ExceptionDescriptor FromException(Exception exception)
        {
            exception = exception.GetInnermostException();
            if(exception != null)
            {
                int? linenumber = (new StackTrace(exception, true))?.GetFrame(0)?.GetFileLineNumber();

                return new ExceptionDescriptor
                {
                    Name = exception.GetType().Name,
                    Message = exception.Message
                };
            }
            return null;
        }
    }
}

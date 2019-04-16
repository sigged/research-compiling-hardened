using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sigged.CsCNetCore.Web.Extensions
{
    public static class ExceptionExtensions
    {
        public static Exception GetInnermostException(this Exception exception)
        {
            if(exception?.InnerException != null)
            {
                return exception.InnerException.GetInnermostException();
            }
            return exception;
        }
    }
}

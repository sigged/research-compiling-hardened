using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sigged.Repl.NetCore.Web.Models
{
    public class BuildResultDto
    {
        public string SessionId { get; set; }

        public bool IsSuccess { get; set; }
        
        public IEnumerable<BuildErrorDto> BuildErrors { get; set; }
    }
}

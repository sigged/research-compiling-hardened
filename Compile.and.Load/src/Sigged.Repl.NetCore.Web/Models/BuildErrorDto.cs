using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sigged.Repl.NetCore.Web.Models
{
    public class BuildErrorDto
    {
        public string Severity { get; set; }
        public string Id { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
    }
}

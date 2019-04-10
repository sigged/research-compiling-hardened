using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Sigged.Repl.NetCore.Web.Models
{
    public class BuildErrorDto
    {
        public string Severity { get; set; }
        public string Id { get; set; }
        public LinePosition StartPosition { get; set; }
        public LinePosition EndPosition { get; set; }
        public string Description { get; set; }
    }
}

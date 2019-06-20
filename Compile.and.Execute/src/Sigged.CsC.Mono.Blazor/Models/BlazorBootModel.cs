namespace Sigged.CsC.Mono.Blazor.Models
{
    public class BlazorBootModel
    {
        public string main { get; set; }
        public string entryPoint { get; set; }
        public string[] assemblyReferences { get; set; }
        public string[] cssReferences { get; set; }
        public string[] jsReferences { get; set; }
        public bool linkerEnabled { get; set; }
    }
}

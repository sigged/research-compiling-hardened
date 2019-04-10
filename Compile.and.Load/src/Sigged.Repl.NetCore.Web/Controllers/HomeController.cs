using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Sigged.Compiling.Core;
using Sigged.Repl.NetCore.Web.Models;

namespace Sigged.Repl.NetCore.Web.Controllers
{
    public class HomeController : Controller
    {
        private IHostingEnvironment env;
        private Compiler compiler;

        public HomeController(IHostingEnvironment henv)
        {
            env = henv;

            string netstandardRefsDirectory = Path.Combine(env.ContentRootPath, "_libs", "netstandard2.0");
            compiler = new Compiler(netstandardRefsDirectory);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CodeView()
        {
            List<string> themes = new List<string>();
            var dir = new DirectoryInfo(Path.Combine(env.WebRootPath, "js", "codemirror", "theme"));
            foreach (var file in dir.GetFiles("*.css"))
            {
                themes.Add(Path.GetFileNameWithoutExtension(file.FullName));
            }
            ViewBag.Themes = themes;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Build([FromBody] BuildRequestDto buildRequest)
        {
            BuildResultDto result = new BuildResultDto();

            using (var stream = new MemoryStream())
            {
                EmitResult results = await compiler.Compile(buildRequest.SourceCode, "REPLAssembly", stream);
                result.BuildErrors = results.Diagnostics.Select(d =>
                    new BuildErrorDto
                    {
                        Id = d.Id,
                        Severity = d.Severity.ToString()?.ToLower(),
                        Description = d.GetMessage(),
                        StartPosition = d.Location.GetLineSpan().StartLinePosition,
                        EndPosition = d.Location.GetLineSpan().EndLinePosition
                    }
                );
                result.IsSuccess = results.Success;
            }
            return Json(result);
        }

        private string GetCodeLocation(Location codeLocation)
        {
            string result = "";
            var pos = codeLocation.GetLineSpan();
            if (pos.Path != null)
            {
                // user-visible line and column counts are 1-based, but internally are 0-based.
                result += (pos.StartLinePosition.Line + 1) + ":" + (pos.StartLinePosition.Character + 1);
            }

            return result;
        }
        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

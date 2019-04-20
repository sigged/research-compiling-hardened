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
using Newtonsoft.Json;
using Sigged.Compiling.Core;
using Sigged.CsC.CodeSamples.Parser;
using Sigged.CsC.NetCore.Web.Models;
using Sigged.CsC.NetCore.Web.Services;

namespace Sigged.CsC.NetCore.Web.Controllers
{
    public class HomeController : Controller
    {
        private IHostingEnvironment env;

        public HomeController(IHostingEnvironment henv)
        {
            env = henv;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetCodeSamples()
        {
            var items = await SampleParser.GetSamples(onlyMetaData: true);

            var categories = items.Select(cs => cs.Category).Distinct().ToList();
            var grouped = new List<CodeSampleCategory>();

            foreach(var cat in categories)
            {
                grouped.Add(new CodeSampleCategory
                {
                    Category = cat,
                    Samples = items.Where(cs => cs.Category == cat)
                });
            }
            
            return Json(grouped);
        }

        public async Task<IActionResult> GetCodeSample(string id)
        {
            var cs = (await SampleParser.GetSamples()).FirstOrDefault(s => s.Id == id);
            return Json(cs);
        }

        public IActionResult CodeView()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

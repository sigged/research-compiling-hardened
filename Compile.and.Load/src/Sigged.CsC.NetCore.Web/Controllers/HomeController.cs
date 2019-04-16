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
using Sigged.CsCNetCore.Web.Models;
using Sigged.CsCNetCore.Web.Services;

namespace Sigged.CsCNetCore.Web.Controllers
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

        public IActionResult CodeView()
        {
            //List<string> themes = new List<string>();
            //var dir = new DirectoryInfo(Path.Combine(env.WebRootPath, "js", "codemirror", "theme"));
            //foreach (var file in dir.GetFiles("*.css"))
            //{
            //    themes.Add(Path.GetFileNameWithoutExtension(file.FullName));
            //}
            //ViewBag.Themes = themes;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

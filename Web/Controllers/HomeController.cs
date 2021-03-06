﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Web.Models;
using VoidDetector;
using System.IO;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public IActionResult Index()
        {
            VoidDetector.Program.Learn();
            return View();
        }

        public IActionResult Process()
        {
            var file = Request.Form.Files;
            List<Results> results = VoidDetector.Program.Start(file[0].FileName);

            ResumViewModel resumViewModel = new ResumViewModel(results);

            resumViewModel.FileName = file[0].FileName;
            resumViewModel.Obstruction = Convert.ToInt32(results.Where(x => x.prediction == "obstruction").Count() * 100 / 79);
            resumViewModel.ObstructionStr = resumViewModel.Obstruction > 0 ? "Si" : "No";
            resumViewModel.Empty = results.Where(x => x.prediction == "empty").Count();
            resumViewModel.EmptyPercent = Convert.ToInt32(results.Where(x => x.prediction == "empty").Count() * 100 / 79);
            resumViewModel.Full = Convert.ToInt32(results.Where(x => x.prediction == "full").Count()* 100 / 79) + resumViewModel.EmptyPercent;

            return View(resumViewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

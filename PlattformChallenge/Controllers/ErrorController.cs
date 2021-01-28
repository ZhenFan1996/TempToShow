using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using PlattformChallenge.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PlattformChallenge.Controllers
{
    public class ErrorController :Controller
    {
        private ILogger<ErrorController> logger;
        private readonly IStringLocalizer<ErrorController> _localizer;


        public ErrorController(ILogger<ErrorController> logger,IStringLocalizer<ErrorController> localizer)
        {
            this.logger = logger;
            this._localizer = localizer;
        }
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            if (statusCodeResult == null) {
                ViewBag.ErrorMessage = _localizer["404"];
                return View("NotFound");
            }
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = _localizer["404"];                   
                    logger.LogWarning(_localizer["Info"] +
                $"{statusCodeResult.OriginalPath}" + _localizer["Query"]+
                $"{statusCodeResult.OriginalQueryString}");
                    break;
            }
            return View("NotFound");
        }

        [Route("Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            ViewBag.ErrorMessage = exceptionHandlerPathFeature.Error.Message;
            logger.LogError($"Path:{exceptionHandlerPathFeature.Path},ErrorMessge{exceptionHandlerPathFeature.Error}"); 
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

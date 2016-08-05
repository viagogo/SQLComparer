using Microsoft.AspNetCore.Mvc;
using NLog;
using NLog.Targets;
using SqlComparer.Web.ViewModels;
using System.Linq;
using SqlComparer.Web.Models;

namespace SqlComparer.Web.Controllers
{
    public class LogController : Controller
    {
        public IActionResult Index()
        {
            var target = (MemoryTarget) LogManager.Configuration.FindTargetByName("memLogger");

            var viewModel = new LogHistory
            {
                LogMessages = target.Logs.Select(LogMessage.FromString)
            };

            return View(viewModel);
        }
    }
}

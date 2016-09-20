using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SqlComparer.Web.Services;
using SqlComparer.Web.ViewModels;

namespace SqlComparer.Web.Controllers
{
    [Authorize(Policy = "MinimumAccessPermission")]
    public class ComparisonController : Controller
    {
        private readonly ISqlComparerService _sqlComparerService;
        private readonly IOptionsService _optionsService;
        private readonly ILogger<ComparisonController> _logger;

        public ComparisonController(
            ISqlComparerService sqlComparerService, 
            IOptionsService optionsService, 
            ILogger<ComparisonController> logger)
        {
            _sqlComparerService = sqlComparerService;
            _optionsService = optionsService;
            _logger = logger;
        }

        public IActionResult CompareProcs()
        {
            return View(new ComparisonResultViewModel());
        }

        [HttpPost]
        public IActionResult CompareProcs(ComparisonResultViewModel comparisonResultViewModel)
        {
            var comparisonResult = _sqlComparerService.Compare(
                    comparisonResultViewModel.LeftEntity,
                    comparisonResultViewModel.RightEntity,
                    "Left source",
                    "Right source",
                    targetDatabase: null);

            if (comparisonResult == null)
            {
                TempData["message"] = "Something went wrong. Consult the logs for more information.";
                return View(comparisonResultViewModel);
            }

            comparisonResult.ShowPushOptions = false;
            return View(comparisonResult);
        }

        [HttpPost]
        public IActionResult Push(ComparisonResultViewModel comparisonResultViewModel)
        {
            string sqlToPush;
            string targetConnection;
            string origin;
            string target;
            
            if (comparisonResultViewModel.PushFromLeftToRight)
            {
                sqlToPush = comparisonResultViewModel.LeftEntity;
                targetConnection = _optionsService.ConnectionStrings.GetConnectionByAlias(comparisonResultViewModel.RightAlias);
                origin = comparisonResultViewModel.LeftAlias;
                target = comparisonResultViewModel.RightAlias;
            }
            else if (comparisonResultViewModel.PushFromRightToLeft)
            {
                sqlToPush = comparisonResultViewModel.RightEntity;
                targetConnection = _optionsService.ConnectionStrings.GetConnectionByAlias(comparisonResultViewModel.LeftAlias);
                origin = comparisonResultViewModel.RightAlias;
                target = comparisonResultViewModel.LeftAlias;
            }
            else
            {
                _logger.LogError("No push direction specified");
                return View("Error");
            }

            var connectionAlias = _optionsService.ConnectionStrings.GetAliasByConnection(targetConnection);
            if (!_optionsService.Permissions.CanPushToConnectionAliases(User, new[] {connectionAlias}))
            {
                TempData["message"] = $"No permission to push from {origin} to {target}";
                _logger.LogError($"No permission to push from {origin} to {target}");
                return RedirectToAction("Index", "Search");
            }

            if (!string.IsNullOrWhiteSpace(comparisonResultViewModel.TargetDatabase))
            {
                // TODO: Should probably formalise 'base' and 'target' entities early on in this method and reuse
                if (string.IsNullOrWhiteSpace(comparisonResultViewModel.PushFromLeftToRight ? comparisonResultViewModel.RightEntity : comparisonResultViewModel.LeftEntity))
                {
                    // Proc doesn't exist yet in environment -- create proc as-is
                    if(_sqlComparerService.InsertEntity(sqlToPush, comparisonResultViewModel.TargetDatabase, targetConnection))
                    {
                        TempData["message"] = $"Created new procedure from {origin} to {target}";
                        _logger.LogInformation($"Created new procedure {sqlToPush} from {origin} to {target}");
                        return RedirectToAction("Index", "Search");
                    }
                }
                else
                {
                    // Proc already exists, do an alter statement
                    if (_sqlComparerService.AlterExistingEntity(sqlToPush, comparisonResultViewModel.TargetDatabase, targetConnection))
                    {
                        TempData["message"] = $"Altered existing entity from {origin} to {target}";
                        _logger.LogInformation($"Altered existing entity {sqlToPush} from {origin} to {target}");
                        return RedirectToAction("Index", "Search");
                    }
                }
                
    
                TempData["message"] = $"Could not push from {origin} to {target}";
                _logger.LogError($"Could not push {sqlToPush} from {origin} to {target}. Refer to the log for more information.");            

                return RedirectToAction("Index", "Search");
            }

            _logger.LogError($"Something went wrong. Database: {comparisonResultViewModel.TargetDatabase}, Sql:\n{sqlToPush}");
            TempData["message"] = "Something went really, really wrong. Whatever you did, try it again.";
            return RedirectToAction("Index", "Search");
        }
    }
}

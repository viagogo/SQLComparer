using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SqlComparer.Model;
using SqlComparer.Web.Services;
using SqlComparer.Web.ViewModels;

namespace SqlComparer.Web.Controllers
{
    [Authorize(Policy = "MinimumAccessPermission")]
    public class SearchController : Controller
    {
        private readonly IDatabaseEntityRepository _databaseEntityRepository;
        private readonly IIdentifierService _identifierService;
        private readonly ILogger<SearchController> _logger;
        private readonly IOptionsService _optionsService;
        private readonly ISqlComparerService _sqlComparerService;

        public SearchController(
            IDatabaseEntityRepository databaseEntityRepository,
            ISqlComparerService sqlComparerService,
            IIdentifierService identifierService,
            IOptionsService optionsService,
            ILogger<SearchController> logger)
        {
            _databaseEntityRepository = databaseEntityRepository;
            _sqlComparerService = sqlComparerService;
            _identifierService = identifierService;
            _optionsService = optionsService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var searchModel = new Search
            {
                ConfigConnections = _optionsService.ConnectionStrings.GetConnectionFilters().ToList()
            };

            return View(searchModel);
        }

        [HttpPost]
        public IActionResult Search(Search searchQuery)
        {
            if (searchQuery.ObjectName == null)
            {
                TempData["message"] = "Enter a search query";
                return View("Index", searchQuery);
            }

            var includedConnectionStrings = _optionsService.ConnectionStrings.GetIncludedConnectionStrings(searchQuery.ConfigConnections).ToArray();
            var objectIdentifiers = _identifierService.GetIdentifiersFromObjectNames(searchQuery.ObjectName.Split(',').Select(x => x.Trim())).ToList();
            searchQuery.ComparisonResults = GetComparisonsBetweenIdentifiers(includedConnectionStrings, objectIdentifiers);
            return View("Index", searchQuery);
        }

        public IActionResult CompareSchema()
        {
            var schemaComparisonModel = new SchemaComparison
            {
                ConfigConnections = _optionsService.ConnectionStrings.GetConnectionFilters().ToList()
            };

            return View("CompareSchema", schemaComparisonModel);
        }

        [HttpPost]
        public IActionResult CompareSchema(SchemaComparison schemaComparison)
        {
            var includedConnectionStrings = _optionsService.ConnectionStrings.GetIncludedConnectionStrings(schemaComparison.ConfigConnections).ToArray();
            var identifiers = _databaseEntityRepository.GetIdentifiersFromSchema(_identifierService.GetIdentifierFromSchemaName(schemaComparison.SchemaName), includedConnectionStrings).Take(20);
            schemaComparison.ComparisonResults = GetComparisonsBetweenIdentifiers(includedConnectionStrings, identifiers);
            return View("CompareSchema", schemaComparison);
        }

        private IDictionary<ObjectIdentifier, IList<ComparisonResultViewModel>> GetComparisonsBetweenIdentifiers(
            IEnumerable<string> connectionStrings, 
            IEnumerable<ObjectIdentifier> identifiers)
        {
            var results = new Dictionary<ObjectIdentifier, IList<ComparisonResultViewModel>>();
            
            foreach (var identifier in identifiers)
            {
                _logger.LogInformation($"Searching for object {identifier}");
                
                foreach (var connectionString in connectionStrings)
                {
                    var connectionStringCopies = connectionStrings.ToList();
                    var baseProc = _databaseEntityRepository.GetStoredProcedure(identifier, connectionString);
                    var leftAlias = _optionsService.ConnectionStrings.GetAliasByConnection(connectionString);

                    foreach (var connectionStringCopy in connectionStringCopies)
                    {
                        var variableProc = _databaseEntityRepository.GetStoredProcedure(identifier, connectionStringCopy);
                        var rightAlias = _optionsService.ConnectionStrings.GetAliasByConnection(connectionStringCopy);

                        if (!results.ContainsKey(identifier))
                        {
                            results.Add(identifier, new List<ComparisonResultViewModel>());
                        }

                        results[identifier].Add(
                            _sqlComparerService.Compare(
                                baseProc.Representation,
                                variableProc.Representation,
                                leftAlias,
                                rightAlias,
                                identifier.Database,
                                isEncrypted: baseProc.IsEncrypted || variableProc.IsEncrypted));
                    }
                }
            }

            TempData["message"] = $"Searched for objects {string.Join(", ", identifiers)}";

            return results;
        }
    }
}
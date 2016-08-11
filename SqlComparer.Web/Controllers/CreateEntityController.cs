using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SqlComparer.Model.DatabaseEntities;
using SqlComparer.Web.Services;
using SqlComparer.Web.ViewModels;

namespace SqlComparer.Web.Controllers
{
    [Authorize(Policy = "MinimumAccessPermission")]
    public class CreateEntityController : Controller
    {
        private readonly IDatabaseEntityRepository _databaseEntityRepository;
        private readonly IIdentifierService _identifierService;
        private readonly ILogger<CreateEntityController> _logger;
        private readonly IOptionsService _optionsService;
        private readonly ISqlComparerService _sqlComparerService;

        public CreateEntityController(
            IDatabaseEntityRepository databaseEntityRepository,
            IOptionsService optionsService,
            ISqlComparerService sqlComparerService,
            IIdentifierService identifierService,
            ILogger<CreateEntityController> logger)
        {
            _databaseEntityRepository = databaseEntityRepository;
            _optionsService = optionsService;
            _sqlComparerService = sqlComparerService;
            _identifierService = identifierService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var model = new CreateEntity();
            model.Connections = _optionsService.ConnectionStrings.GetConnectionFilters().ToList();
            model.ExistingDatabases = _databaseEntityRepository.GetCommonDatabases(_optionsService.ConnectionStrings.GetConnectionStrings()).OrderBy(x => x);
            model.Database = model.ExistingDatabases.Contains(_optionsService.DatabaseSettings.DefaultDatabase) ? _optionsService.DatabaseSettings.DefaultDatabase : null;

            return View(model);
        }

        [HttpPost]
        public IActionResult CreateEntity(CreateEntity createEntity)
        {
            var includedConnectionStrings = _optionsService.ConnectionStrings.GetIncludedConnectionStrings(createEntity.Connections);
            if (createEntity.ExternalConnection != null)
            {
                includedConnectionStrings = includedConnectionStrings.Concat(new[] {createEntity.ExternalConnection});
            }

            var connectionAliases = _optionsService.ConnectionStrings.GetAliasesFromConnections(includedConnectionStrings);
            if (!_optionsService.Permissions.CanPushToConnectionAliases(User, connectionAliases))
            {
                _logger.LogError("Could not push due to insufficient permissions.");
                _logger.LogInformation($"Targets: {string.Join(", ", connectionAliases)}");
                TempData["successCreate"] = false;
                TempData["message"] = "You don't have sufficient permissions to push to the selected targets";
                return View("Index", createEntity);
            }

            var sourcesToInsert = new List<string>();
            foreach (var connectionString in includedConnectionStrings)
            {
                var identifier = _identifierService.GetProcedureIdentifierFromSql(createEntity.Sql).WithDatabase(createEntity.Database);
                var entityAlreadyExists = _sqlComparerService.ProcedureExists(identifier, connectionString);

                if (entityAlreadyExists)
                {
                    if (createEntity.ForceCreate)
                    {
                        _sqlComparerService.AlterExistingEntity(createEntity.Sql, createEntity.Database, connectionString);
                    }
                    else
                    {
                        // Get all existing entities
                        IEnumerable<StoredProcedure> existingProcedures;
                        try
                        {
                            existingProcedures = _databaseEntityRepository.GetStoredProcedures(identifier, includedConnectionStrings).Where(x => x.Exists);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"An exception occurred when retrieving procedures for identifier {identifier}");
                            _logger.LogError($"{e}");
                            return View("Index", createEntity);
                        }

                        // Compare them with new sql
                        foreach (var existingProcedure in existingProcedures)
                        {
                            createEntity.ExistingEntities.Add(
                                _sqlComparerService.Compare(
                                    existingProcedure.Representation,
                                    createEntity.Sql,
                                    _optionsService.ConnectionStrings.GetAliasByConnection(
                                        existingProcedure.ConnectionString),
                                    "New entity",
                                    identifier.Database));
                        }

                        // Return with error message
                        _logger.LogError($"The entity {identifier} is already present in one or more targets.");
                        TempData["successCreate"] = false;
                        TempData["message"] = $"The entity {identifier} is already present in one or more targets.";
                        return View("Index", createEntity);
                    }
                }
                else
                {
                    sourcesToInsert.Add(connectionString);
                }
            }

            var success = _databaseEntityRepository.InsertEntities(createEntity.Database, createEntity.Sql, sourcesToInsert);

            TempData["successCreate"] = success;
            if (success)
            {
                _logger.LogInformation($"SQL succesfully executed.\n{createEntity.Sql}");
                TempData["message"] = "SQL succesfully executed.";
            }
            else
            {
                _logger.LogError($"An error occurred while executing the SQL.");
                TempData["message"] =
                    "An error occurred while executing the SQL. More information can be found in the logs.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
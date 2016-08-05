using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using SqlComparer.Model;
using SqlComparer.Util;
using SqlComparer.Web.ViewModels;

namespace SqlComparer.Web.Services.Implementation
{
    internal class SqlComparerService : ISqlComparerService
    {
        private readonly IComparer _comparer;
        private readonly IComparedEntityFactory _entityFactory;
        private readonly IDatabaseEntityRepository _databaseEntityRepository;
        private readonly ITSqlFragmentFactory _fragmentFactory;
        private readonly IOptionsService _optionsService;
        private readonly ILogger<SqlComparerService> _logger;

        public SqlComparerService(
            IComparer comparer, 
            IComparedEntityFactory entityFactory, 
            IDatabaseEntityRepository databaseEntityRepository, 
            ITSqlFragmentFactory fragmentFactory,
            IOptionsService optionsService,
            ILogger<SqlComparerService> logger)
        {
            _comparer = comparer;
            _entityFactory = entityFactory;
            _databaseEntityRepository = databaseEntityRepository;
            _fragmentFactory = fragmentFactory;
            _optionsService = optionsService;
            _logger = logger;
        }

        public ComparisonResultViewModel Compare(string leftEntity, string rightEntity, string leftAlias, string rightAlias, string targetDatabase, bool isEncrypted = false)
        {
            ComparedEntity leftComparisonEntity;
            ComparedEntity rightComparisonEntity;
            try
            {
                leftComparisonEntity = _entityFactory.Create(leftEntity);
                rightComparisonEntity = _entityFactory.Create(rightEntity);
            } catch(Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogError(e.StackTrace);
                return null;
            }

            var compareOptions = new CompareOptions
            {
                IgnoreEndOfFile = _optionsService.ComparisonSettings.IgnoreEndOfFile,
                IgnoreNewLines = _optionsService.ComparisonSettings.IgnoreNewLines,
                IgnoreWhitespace = _optionsService.ComparisonSettings.IgnoreWhitespace,
                CaseInsensitive = _optionsService.ComparisonSettings.CaseInsensitive
            };

            var comparisonResult = _comparer.Compare(compareOptions, leftComparisonEntity, rightComparisonEntity).Single();
            if (leftAlias == rightAlias)
            {
                comparisonResult.Outcome = ComparisonOutcome.SameObject;
            }

            return new ComparisonResultViewModel
            {
                ComparisonResult = comparisonResult,
                LeftAlias = leftAlias,
                RightAlias = rightAlias,
                TargetDatabase = targetDatabase,
                LeftEntity = leftEntity,
                RightEntity = rightEntity,
                IsEncrypted = isEncrypted,
            };
        }

        public bool ProcedureExists(ObjectIdentifier identifier, string connectionString)
        {
            return _databaseEntityRepository.EntityExists(identifier, connectionString);
        }

        public bool AlterExistingEntity(string sql, string database, string connectionString)
        {
            var alteredStatement = _fragmentFactory.GetAlterProcedureStatement(sql);
            var newSql = alteredStatement.Fragment.ToSourceString();

            return _databaseEntityRepository.AlterExistingEntity(newSql, database, connectionString);
        }

        public bool InsertEntity(string sql, string database, string connectionString)
        {
            return _databaseEntityRepository.InsertEntity(database, sql, connectionString);
        }
    }
}

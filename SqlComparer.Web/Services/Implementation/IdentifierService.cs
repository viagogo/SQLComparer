using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlComparer.Model;
using SqlComparer.Util;
using SqlComparer.Web.Models.Options;

namespace SqlComparer.Web.Services.Implementation
{
    public class IdentifierService : IIdentifierService
    {
        private readonly ITSqlFragmentFactory _fragmentFactory;
        private readonly IOptions<DatabaseSettings> _databaseSettings;
        private readonly ILogger<IdentifierService> _logger;

        public IdentifierService(ITSqlFragmentFactory fragmentFactory, IOptions<DatabaseSettings> databaseSettings, ILogger<IdentifierService> logger)
        {
            _fragmentFactory = fragmentFactory;
            _databaseSettings = databaseSettings;
            _logger = logger;
        }

        public ObjectIdentifier GetProcedureIdentifierFromSql(string sql)
        {
            // Extract identifier from syntax elements
            // Create identifier object
            // Object might be partially filled with data (e.g. only name)
            // Hence we serialize it and fill it up with default values where needed
            var objectName = Helpers.GetIdentifierFromSqlProcedure(_fragmentFactory.CreateFragment(sql).Fragment).ToString();
            return GetIdentifierFromObjectName(objectName);
        }

        public ObjectIdentifier GetIdentifierFromSchemaName(string schemaName)
        {
            // We use a workaround by appending a temporary object name and removing it afterwards
            // This allows us to re=use the existing logic to get the identifier from an object name
            // Example inputs:
            // db.schema -> db.schema.temp
            // schema -> schema.temp
            
            var temporaryName = schemaName + ".temp";
            var identifier = GetIdentifierFromObjectName(temporaryName);
            identifier.Name = null;
            return identifier;
        }

        public IEnumerable<ObjectIdentifier> GetIdentifiersFromObjectNames(IEnumerable<string> objectNames)
        {
            foreach (var objectName in objectNames)
            {
                yield return GetIdentifierFromObjectName(objectName);
            }
        }

        public ObjectIdentifier GetIdentifierFromObjectName(string objectName)
        {
            var identifier = StripObjectName(objectName);
            if (string.IsNullOrWhiteSpace(identifier.Database))
            {
                identifier.Database = _databaseSettings.Value.DefaultDatabase;
            }

            if (string.IsNullOrWhiteSpace(identifier.Schema))
            {
                identifier.Schema = _databaseSettings.Value.DefaultSchema;
            }

            return identifier;
        }

        private ObjectIdentifier StripObjectName(string objectName)
        {
            var tokens = objectName.Split('.');
            string db = null;
            string schema = null;
            string name = null;
            if (tokens.Length == 3)
            {
                db = tokens[0];
                schema = tokens[1];
                name = tokens[2];
            } else if (tokens.Length == 2)
            {
                schema = tokens[0];
                name = tokens[1];
            }
            else if (tokens.Length == 1)
            {
                name = tokens[0];
            }
            else
            {
                _logger.LogError($"Could not parse object name '{objectName}'");
                throw new ArgumentException($"Could not parse object name '{objectName}'", nameof(objectName));
            }

            return new ObjectIdentifier
            {
                Database = db,
                Schema = schema,
                Name = name
            };
        }
    }
}

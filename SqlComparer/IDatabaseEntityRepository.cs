using System.Collections.Generic;
using SqlComparer.Model;
using SqlComparer.Model.DatabaseEntities;

namespace SqlComparer
{
    public interface IDatabaseEntityRepository
    {
        IEnumerable<StoredProcedure> GetStoredProcedures(ObjectIdentifier identifier, IEnumerable<string> connectionStrings);

        StoredProcedure GetStoredProcedure(ObjectIdentifier identifier, string connectionString);

        bool InsertEntities(string database, string sql, IEnumerable<string> connectionStrings);

        bool InsertEntity(string database, string sql, string connectionString);

        bool EntityExists(ObjectIdentifier objectIdentifier, string connectionString);

        IEnumerable<Database> GetDatabases(string connectionString);

        IEnumerable<string> GetCommonDatabases(IEnumerable<string> connectionStrings);

        bool AlterExistingEntity(string sql, string database, string connectionString);

        IEnumerable<ObjectIdentifier> GetIdentifiersFromSchema(ObjectIdentifier schemaIdentifier, IEnumerable<string> connectionStrings);

        IEnumerable<ObjectIdentifier> GetIdentifiersFromSchema(ObjectIdentifier schemaIdentifier, string connectionString);
    }
}

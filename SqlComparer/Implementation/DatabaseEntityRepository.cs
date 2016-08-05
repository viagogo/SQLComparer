using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NLog;
using SqlComparer.Model;
using SqlComparer.Model.DatabaseEntities;

namespace SqlComparer.Implementation
{
    public class DatabaseEntityRepository : IDatabaseEntityRepository
    {
        private readonly IConnectionService _connectionService;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DatabaseEntityRepository(IConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public IEnumerable<StoredProcedure> GetStoredProcedures(ObjectIdentifier identifier, IEnumerable<string> connectionStrings)
        {
            foreach (var connectionString in connectionStrings)
            {
                yield return GetStoredProcedure(identifier, connectionString);
            }
        }

        public StoredProcedure GetStoredProcedure(ObjectIdentifier identifier, string connectionString)
        {
            var sql = $@"select OBJECT_DEFINITION((select OBJECT_ID('{identifier}'))) as 'definition'";

            return ExecuteReader(sql, connectionString, (reader) =>
            {
                var result = new StoredProcedure
                {
                    ConnectionString = connectionString,
                    Identifier = identifier
                };

                if (reader == null || !reader.HasRows)
                {
                    return result;
                }

                reader.Read();
                result.Representation = reader["definition"].ToString();
                if (string.IsNullOrWhiteSpace(result.Representation))
                {
                    // Try again to see if we can find the object ID
                    // If we do find an ID but we can't find the definition, it means the proc is encrypted.

                    sql = $@"select OBJECT_ID('{identifier}') as 'id'";
                    var exists = ExecuteReader(sql, connectionString, (idReader) =>
                    {
                        if (idReader == null || !reader.HasRows)
                        {
                            return false;
                        }

                        idReader.Read();
                        var id = idReader["id"] as int?;
                        return id.HasValue;
                    }, identifier.Database);

                    if (exists)
                    {
                        result.IsEncrypted = true;
                        result.Exists = true;
                    }

                    return result;
                }

                result.Exists = true;
                return result;
            }, identifier.Database);
        }

        public bool InsertEntities(string database, string sql, IEnumerable<string> connectionStrings)
        {
            foreach (var connectionString in connectionStrings)
            {
                if (!InsertEntity(database, sql, connectionString))
                {
                    return false;
                }
            }

            return true;
        }

        public bool InsertEntity(string database, string sql, string connectionString)
        {
            return ExecuteNonQuery(sql, database, connectionString);
        }

        public bool EntityExists(ObjectIdentifier identifier, string connectionString)
        {
            var sql = $@"
select OBJECT_ID('{identifier}') as 'object_id'";

            return ExecuteReader(sql, connectionString, (reader) =>
            {
                reader.Read();
                return !reader.IsDBNull(0);
            }, identifier.Database);
        }

        public IEnumerable<Database> GetDatabases(string connectionString)
        {
            var sql = @"
select name
from sys.databases
where 
      state = 0
  and source_database_id is null
  and is_read_only = 0
  and not
	(
	 is_distributor = 1
	 or name in ('master', 'tempdb', 'model', 'msdb')
	)
";

            return ExecuteReader(sql, connectionString, (reader) =>
            {
                if (reader == null)
                {
                    return Enumerable.Empty<Database>();
                }

                var foundDatabases = new List<Database>();
                while (reader.Read())
                {
                    foundDatabases.Add(new Database
                    {
                        Name = (string) reader["name"]
                    });
                }

                return foundDatabases;
            });
        }

        public IEnumerable<string> GetCommonDatabases(IEnumerable<string> connectionStrings)
        {
            return connectionStrings.SelectMany(GetDatabases)
                .GroupBy(x => x.Name, (name, databases) => new
                {
                    Name = name,
                    Count = databases.Count()
                }).Where(x => x.Count == connectionStrings.Count())
                .Select(x => x.Name);
        }

        public bool AlterExistingEntity(string sql, string database, string connectionString)
        {
            return ExecuteNonQuery(sql, database, connectionString);
        }

        public IEnumerable<ObjectIdentifier> GetIdentifiersFromSchema(ObjectIdentifier schemaIdentifier, IEnumerable<string> connectionStrings)
        {
            // We filter the objects in-memory because we don't want to receive the same identifier 
            // multiple times because it's found in multiple connections
            var resultSet = new HashSet<ObjectIdentifier>();

            foreach (var connectionString in connectionStrings)
            {
                foreach (var procedureIdentifier in GetIdentifiersFromSchema(schemaIdentifier, connectionString))
                {
                    if (!resultSet.Contains(procedureIdentifier))
                    {
                        resultSet.Add(procedureIdentifier);
                    }
                }
            }

            return resultSet;
        }

        public IEnumerable<ObjectIdentifier> GetIdentifiersFromSchema(ObjectIdentifier schemaIdentifier, string connectionString)
        {
            var sql = $@"
select o.Name as 'ObjectName', s.Name as 'SchemaName'
from sys.objects o
join sys.schemas s
	on o.schema_id = s.schema_id
where s.Name = '{schemaIdentifier.Schema}'
    and o.type = 'P'
";
            
            return ExecuteReader(sql, connectionString, (reader) =>
            {
                if (reader == null)
                {
                    return Enumerable.Empty<ObjectIdentifier>();
                }

                var foundIdentifiers = new List<ObjectIdentifier>();
                while (reader.Read())
                {
                    foundIdentifiers.Add(new ObjectIdentifier
                    {
                        Database = schemaIdentifier.Database,
                        Schema = schemaIdentifier.Schema,
                        Name = (string) reader["ObjectName"]
                    });
                }
                return foundIdentifiers;
            }, schemaIdentifier.Database);
        }

        private bool ExecuteNonQuery(string sql, string database, string connectionString)
        {
            using (var conn = _connectionService.GetConnection(connectionString, database))
            {
                try
                {
                    conn.Open();
                }
                catch (Exception e)
                {
                    Logger.Error(e, $"Could not open connection {connectionString}");
                    Logger.Error(e.Message);
                    Logger.Error(e.StackTrace);
                    return false;
                }

                using (var command = new SqlCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.Connection = conn;

                    // Only for dev db
                    // Temporary but useful during development so we can test with dev & qa db
                    try
                    {
                        command.CommandText = $"exec SetContextInfoToProductBacklogItem  @ProductBacklogItemID = 69409";
                        command.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                    }

                    try
                    {
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, $"Could not execute {command.CommandText}");
                        Logger.Error(e.Message);
                        Logger.Error(e.StackTrace);
                        return false;
                    }
                }
            }

            return true;
        }

        private T ExecuteReader<T>(string sql, string connectionString, Func<SqlDataReader, T> func, string database = null)
        {
            using (var conn = _connectionService.GetConnection(connectionString, database))
            {
                try
                {
                    conn.Open();
                }
                catch (Exception e)
                {
                    Logger.Error(e, $"Could not open connection {connectionString}");
                    Logger.Error(e.Message);
                    Logger.Error(e.StackTrace);
                    return default(T);
                }

                using (var command = new SqlCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.Connection = conn;

                    SqlDataReader reader;
                    try
                    {
                        command.CommandText = sql;
                        reader = command.ExecuteReader();
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, $"Could not execute {command.CommandText}");
                        Logger.Error(e.Message);
                        Logger.Error(e.StackTrace);
                        return default(T);
                    }

                    return func(reader);
                }
            }
        }
    }
}
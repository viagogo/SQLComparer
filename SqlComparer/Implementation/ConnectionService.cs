using System;
using System.Data.SqlClient;

namespace SqlComparer.Implementation
{
    public class ConnectionService : IConnectionService
    {
        public SqlConnection GetConnection(string connectionString, string database = null)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (database == null)
            {
                return new SqlConnection(connectionString);
            }

            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = database
            };

            return new SqlConnection(builder.ConnectionString);
        }
    }
}
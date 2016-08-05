using System.Data.SqlClient;

namespace SqlComparer
{
    public interface IConnectionService
    {
        SqlConnection GetConnection(string connectionString, string database = null);
    }
}

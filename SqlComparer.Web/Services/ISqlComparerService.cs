using SqlComparer.Model;
using SqlComparer.Web.ViewModels;

namespace SqlComparer.Web.Services
{
    public interface ISqlComparerService
    {
        ComparisonResultViewModel Compare(string leftEntity, string rightEntity, string leftAlias, string rightAlias, string targetDatabase, bool isEncrypted = false);

        bool ProcedureExists(ObjectIdentifier identifier, string connectionString);

        bool AlterExistingEntity(string sql, string database, string connectionString);

        bool InsertEntity(string sql, string database, string connectionString);
    }
}

using System.Collections.Generic;
using SqlComparer.Model;

namespace SqlComparer.Web.Services
{
    public interface IIdentifierService
    {
        ObjectIdentifier GetIdentifierFromObjectName(string objectName);
        IEnumerable<ObjectIdentifier> GetIdentifiersFromObjectNames(IEnumerable<string> objectNames);
        ObjectIdentifier GetProcedureIdentifierFromSql(string sql);
        ObjectIdentifier GetIdentifierFromSchemaName(string schemaName);
    }
}

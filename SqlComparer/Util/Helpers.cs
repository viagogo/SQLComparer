using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlComparer.Model;
using SqlComparer.Parsing;

namespace SqlComparer.Util
{
    public static class Helpers
    {
        public static ObjectIdentifier GetIdentifierFromSqlProcedure(TSqlFragment fragment)
        {
            var identifier = new ObjectIdentifier();
            var symbolVisitor = new ProcedureObjectNameVisitor(identifier);
            fragment.Accept(symbolVisitor);

            return identifier;
        }
    }
}
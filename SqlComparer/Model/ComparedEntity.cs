using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlComparer.Model
{
    public class ComparedEntity
    {
        public ObjectIdentifier Identifier { get; set; } = new ObjectIdentifier();

        public SqlConnection DbConnection { get; set; }

        public string Representation { get; set; }

        public TSqlFragment Tree { get; set; }

        public bool HasParseErrors => ParseErrors.Any();
        public IEnumerable<ParseError> ParseErrors { get; set; }
    }
}

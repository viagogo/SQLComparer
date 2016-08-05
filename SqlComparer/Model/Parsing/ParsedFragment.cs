using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlComparer.Model.Parsing
{
    public class ParsedFragment
    {
        public IEnumerable<ParseError> ParseErrors { get; set; } = Enumerable.Empty<ParseError>();

        public bool HasErrors => ParseErrors.Any();

        public TSqlFragment Fragment { get; set; }
    }
}

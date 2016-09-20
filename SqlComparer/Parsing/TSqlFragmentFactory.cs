using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using NLog;
using SqlComparer.Model.Parsing;

namespace SqlComparer.Parsing
{
    public class TSqlFragmentFactory : ITSqlFragmentFactory
    {
        private readonly TSql130Parser _parser = new TSql130Parser(true);
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ParsedFragment CreateFragment(string sql)
        {
            if (sql == null)
            {
                Logger.Error($"{nameof(sql)} is null");
                throw new ArgumentNullException(nameof(sql));
            }

            IList<ParseError> parseErrors;
            var fragment = _parser.Parse(new StringReader(sql), out parseErrors);

            return new ParsedFragment
            {
                Fragment = fragment,
                ParseErrors = parseErrors
            };
        }

        public ParsedFragment GetAlterProcedureStatement(string sql)
        {
            var fragment = CreateFragment(sql);
            if (fragment.HasErrors)
            {
                Logger.Error($"The given SQL cannot be parsed correctly.\n{string.Join(", ", fragment.ParseErrors)}");
                throw new ArgumentException("The given SQL cannot be parsed correctly.", nameof(sql));
            }

            var stream = fragment.Fragment.ScriptTokenStream;
            var sb = new StringBuilder(stream.Count);

            var firstCreateModified = false;

            for (var i = 0; i < stream.Count; i++)
            {
                var token = stream.ElementAt(i);
                if (token.TokenType == TSqlTokenType.Create && !firstCreateModified)
                {
                    sb.Append("alter");
                    firstCreateModified = true;
                }
                else
                {
                    sb.Append(token.Text);
                }
            }

            var newFragment = CreateFragment(sb.ToString());
            if (newFragment.HasErrors)
            {
                Logger.Error($"Could not create a new tree after replacing all create statements.\n{string.Join(", ", newFragment.ParseErrors)}");
                throw new ArgumentException("Could not create a new tree after replacing all create statements", nameof(sql));
            }

            return newFragment;
        }
    }
}

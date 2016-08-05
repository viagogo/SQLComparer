using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlComparer.Model.Parsing;

namespace SqlComparer
{
    public interface ITSqlFragmentFactory
    {
        ParsedFragment CreateFragment(string sql);

        /// <summary>
        /// Returns a <see cref="TSqlFragment"/> object that represents a <code>alter procedure</code> tree from the given SQL.
        /// Input can be a <code>create procedure</code> and <code>alter procedure</code> tree.
        /// </summary>
        ParsedFragment GetAlterProcedureStatement(string sql);
    }
}

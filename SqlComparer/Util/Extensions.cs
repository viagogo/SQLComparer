using System.Linq;
using System.Text;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlComparer.Util
{
    public static class Extensions
    {
        public static string ToSourceString(this TSqlFragment fragment)
        {
            var stream = fragment.ScriptTokenStream;
            var sb = new StringBuilder(stream.Count);

            for (var i = 0; i < stream.Count; i++)
            {
                sb.Append(stream.ElementAt(i).Text);
            }

            return sb.ToString();
        }
    }
}

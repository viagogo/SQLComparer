using System.Collections.Generic;
using SqlComparer.Model;

namespace SqlComparer.Web.ViewModels
{
    public class SchemaComparison
    {
        public string SchemaName { get; set; }
        public IList<ConnectionFilter> ConfigConnections { get; set; } = new List<ConnectionFilter>();

        /// <summary>
        /// Maps an object identifier to its comparison results across databases
        /// </summary>
        public IDictionary<ObjectIdentifier, IList<ComparisonResultViewModel>> ComparisonResults { get; set; } = new Dictionary<ObjectIdentifier, IList<ComparisonResultViewModel>>();
    }
}

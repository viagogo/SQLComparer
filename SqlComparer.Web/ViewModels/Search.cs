using System.Collections.Generic;
using SqlComparer.Model;

namespace SqlComparer.Web.ViewModels
{
    public class Search
    {
        public string ExternalConnectionString { get; set; }
        public IList<ConnectionFilter> ConfigConnections { get; set; } = new List<ConnectionFilter>();
        
        public string ObjectNames { get; set; }

        /// <summary>
        /// Maps an object identifier to its comparison results across databases
        /// </summary>
        public IDictionary<ObjectIdentifier, IList<ComparisonResultViewModel>> ComparisonResults { get; set; } = new Dictionary<ObjectIdentifier, IList<ComparisonResultViewModel>>();
    }
}

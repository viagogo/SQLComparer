using System;
using System.Collections.Generic;
using System.Linq;
using SqlComparer.Web.ViewModels;

namespace SqlComparer.Web.Models.Options
{
    public class ConnectionStrings
    {
        public string Dev { get; set; }
        public string QA { get; set; }
        public string Prod { get; set; }

        public IEnumerable<Tuple<string, string>> GetConfigConnections()
        {
            return new List<Tuple<string, string>>
            {
                new Tuple<string, string>("dev", Dev),
                new Tuple<string, string>("qa", QA),
                new Tuple<string, string>("prod", Prod)
            }.Where(x => x.Item2 != null);
        }

        public string GetConnectionByAlias(string alias)
        {
            return GetConfigConnections().FirstOrDefault(x => x.Item1 == alias)?.Item2 ?? alias;
        }

        public string GetAliasByConnection(string connection)
        {
            return GetConfigConnections().FirstOrDefault(x => x.Item2 == connection)?.Item1 ?? connection;
        }

        public IEnumerable<string> GetAliasesFromConnections(IEnumerable<string> connectionStrings)
        {
            foreach (var connectionString in connectionStrings)
            {
                yield return GetAliasByConnection(connectionString);
            }
        }

        public IEnumerable<string> GetConnectionsFromAliases(IEnumerable<string> aliases)
        {
            foreach (var alias in aliases)
            {
                yield return GetConnectionByAlias(alias);
            }
        }

        public IEnumerable<ConnectionFilter> GetConnectionFilters()
        {
            foreach (var configConnection in GetConfigConnections())
            {
                yield return new ConnectionFilter
                {
                    ConnectionName = configConnection.Item1,
                    IsIncluded = true
                };
            }
        }

        public IEnumerable<string> GetIncludedConnectionStrings(IEnumerable<ConnectionFilter> connectionFilters)
        {
            foreach (var connectionFilter in connectionFilters.Where(x => x.IsIncluded))
            {
                yield return GetConnectionByAlias(connectionFilter.ConnectionName);
            }
        }

        public IEnumerable<string> GetConnectionStrings()
        {
            return GetConfigConnections().Select(x => x.Item2);
        }
    }
}

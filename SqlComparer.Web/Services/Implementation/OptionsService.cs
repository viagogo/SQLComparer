using Microsoft.Extensions.Options;
using SqlComparer.Web.Models.Options;

namespace SqlComparer.Web.Services.Implementation
{
    public class OptionsService : IOptionsService
    {
        public OptionsService(
            IOptions<ConnectionStrings> connectionStrings, 
            IOptions<DatabaseSettings> databaseSettings, 
            IOptions<Permissions> permissions,
            IOptions<ComparisonSettings> comparisonSettings)
        {
            ConnectionStrings = connectionStrings.Value;
            DatabaseSettings = databaseSettings.Value;
            Permissions = permissions.Value;
            ComparisonSettings = comparisonSettings.Value;
        }

        public ConnectionStrings ConnectionStrings { get; }
        public DatabaseSettings DatabaseSettings { get; }
        public Permissions Permissions { get;}
        public ComparisonSettings ComparisonSettings { get; }
    }
}

using System.Collections.Generic;

namespace SqlComparer.Web.ViewModels
{
    public class CreateEntity
    {
        public IList<ConnectionFilter> Connections { get;set; } = new List<ConnectionFilter>();
        public string ExternalConnection { get; set; }

        public IList<ComparisonResultViewModel> ExistingEntities { get; set; } = new List<ComparisonResultViewModel>();

        public IEnumerable<string> ExistingDatabases { get; set; } = new List<string>();
        public string Database { get; set; }

        /// <summary>
        /// If set to <code>true</code>, any existing entities will be overwritten
        /// </summary>
        public bool ForceCreate { get;set; }

        public string Sql { get; set; } = @"
create procedure dbo.JeroenTest
as
begin
                    
    select top 1 UserId from [API].[NetworkListingIds]

end";
    }
}

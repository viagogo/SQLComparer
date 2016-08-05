using SqlComparer.Model;

namespace SqlComparer.Web.ViewModels
{
    public class ComparisonResultViewModel
    {
        public string LeftEntity { get; set; }
        public string RightEntity { get; set; }

        public string LeftAlias { get; set; }
        public string RightAlias { get; set; }

        public ComparisonResult ComparisonResult { get; set; }

        public bool PushFromRightToLeft { get; set; }
        public bool PushFromLeftToRight { get; set; }

        /// <summary>
        /// Determines whether or not an inline push option should be shown.
        /// Not applicable when the data compared comes from the clipboard rather than an actual data source.
        /// Default: <code>true</code>
        /// </summary>
        public bool ShowPushOptions { get; set; } = true;

        public string TargetDatabase { get; set; }

        /// <summary>
        /// Encrypted comparisons are pointless because we can't inspect the sql code behind it
        /// </summary>
        public bool IsEncrypted { get; set; }
    }
}

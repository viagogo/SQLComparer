namespace SqlComparer.Web.Models.Options
{
    public class ComparisonSettings
    {
        public bool IgnoreWhitespace { get; set; }
        public bool IgnoreNewLines { get; set; }
        public bool IgnoreEndOfFile { get; set; }
        public bool CaseInsensitive { get; set; }
    }
}

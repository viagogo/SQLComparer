namespace SqlComparer.Model
{
    public class CompareOptions
    {
        public bool IgnoreWhitespace { get; set; }
        public bool IgnoreNewLines { get; set; }
        public bool IgnoreEndOfFile { get; set; }
        public bool CaseInsensitive { get; set; }
    }
}

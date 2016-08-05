namespace SqlComparer.Web.ViewModels
{
    public class ConnectionFilter
    {
        public string ConnectionName { get; set; }

        /// <summary>
        /// Determines whether the connection should be used.
        /// Default: true
        /// </summary>
        public bool IsIncluded { get; set; } = true;
    }
}

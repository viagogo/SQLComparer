namespace SqlComparer.Model.DatabaseEntities
{
    public class StoredProcedure
    {
        public string ConnectionString { get; set; }
        public ObjectIdentifier Identifier { get; set; }
        public string Representation { get; set; }
        public bool Exists { get; set; }

        /// <summary>
        /// Stored Procedures that are encrypted cannot be read
        /// </summary>
        public bool IsEncrypted { get; set; }
    }
}

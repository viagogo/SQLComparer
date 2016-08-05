using System.Linq;

namespace SqlComparer.Model
{
    public class ObjectIdentifier
    {
        public string Database { get; set; }
        public string Schema { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return string.Join(".", new [] { Database, Schema, Name }.Where(x => x != null));
        }

        public string ToSimpleName()
        {
            return string.Join(".", new[] {Schema, Name}.Where(x => x != null));
        }

        public ObjectIdentifier WithDatabase(string database)
        {
            return new ObjectIdentifier
            {
                Database = database,
                Schema = Schema,
                Name = Name
            };
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ObjectIdentifier))
            {
                return false;
            }

            return Equals((ObjectIdentifier) obj);
        }

        public bool Equals(ObjectIdentifier o)
        {
            return
                o.Database == Database &&
                o.Schema == Schema &&
                o.Name == Name;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}

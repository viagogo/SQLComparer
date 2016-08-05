using SqlComparer.Web.Models.Options;

namespace SqlComparer.Web.Services
{
    public interface IOptionsService
    {
        ConnectionStrings ConnectionStrings { get; }
        DatabaseSettings DatabaseSettings { get; }
        Permissions Permissions { get; }
        ComparisonSettings ComparisonSettings { get; }
    }
}

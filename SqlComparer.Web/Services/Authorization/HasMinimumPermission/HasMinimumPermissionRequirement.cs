using Microsoft.AspNetCore.Authorization;
using SqlComparer.Web.Models.Options;

namespace SqlComparer.Web.Services.Authorization.HasMinimumPermission
{
    public class HasMinimumPermissionRequirement : IAuthorizationRequirement
    {
        public HasMinimumPermissionRequirement(IOptionsService options)
        {
            Permissions = options.Permissions;
        }

        public Permissions Permissions { get; set; }
    }
}

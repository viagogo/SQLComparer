using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace SqlComparer.Web.Services.Authorization.HasMinimumPermission
{
    public class HasMinimumPermissionHandler : AuthorizationHandler<HasMinimumPermissionRequirement>
    {
        private readonly ILogger<HasMinimumPermissionHandler> _logger;

        public HasMinimumPermissionHandler(ILogger<HasMinimumPermissionHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasMinimumPermissionRequirement requirement)
        {
            _logger.LogInformation($"Is authenticated: {context.User.Identity.IsAuthenticated}");
            _logger.LogInformation($"Identity type: {context.User.Identity.GetType()}");

            var windowsIdentity = context.User.Identity as WindowsIdentity;
            if (windowsIdentity != null)
            {
                _logger.LogInformation($"Is System: {windowsIdentity.IsSystem}");
            }

            var claimsIdentity = context.User.Identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                _logger.LogInformation($"Name: {claimsIdentity.Name}");
            }

            foreach (var permission in requirement.Permissions.MinimumAccess)
            {
                if (context.User.IsInRole(permission))
                {
                    _logger.LogInformation($"Authorization granted as {context.User.Identity.Name}");
                    _logger.LogInformation($"Permission granted on: {permission}");

                    // Can be a different type if ASP.NET is not used
                    var mvcContext = context.Resource as AuthorizationFilterContext;
                    if (mvcContext != null)
                    {
                        _logger.LogInformation($"Host: {mvcContext.HttpContext.Request.Host}");
                        if (mvcContext.HttpContext.Request.Path.HasValue)
                        {
                            _logger.LogInformation($"Path: {mvcContext.HttpContext.Request.Path}");
                        }
                    }
                    
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }

            _logger.LogError($"Authorization denied for {context.User.Identity.Name}");
            _logger.LogInformation($"Accepted roles for minimum access: {string.Join(", ", requirement.Permissions.MinimumAccess)}");

            context.Fail();
            return Task.CompletedTask;
        }
    }
}

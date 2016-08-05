using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace SqlComparer.Web.Models.Options
{
    public class Permissions
    {
        public IList<string> PushToProd { get;set; } = new List<string>();
        public IList<string> PushToQA { get;set; } = new List<string>();
        public IList<string> PushToDev { get;set; } = new List<string>();
        public IList<string> MinimumAccess { get;set; } = new List<string>();

        public bool CanPushToProd(IPrincipal user)
        {
            return IsInRole(user, PushToProd);
        }

        public bool CanPushToQA(IPrincipal user)
        {
            return IsInRole(user, PushToQA);
        }

        public bool CanPushToDev(IPrincipal user)
        {
            return IsInRole(user, PushToDev);
        }

        public bool CanPushToConnectionAliases(IPrincipal user, IEnumerable<string> aliases)
        {
            foreach (var alias in aliases)
            {
                bool canPush = true;

                if (alias == "qa")
                {
                    canPush = CanPushToQA(user);
                } else if (alias == "dev")
                {
                    canPush = CanPushToDev(user);
                } else if (alias == "prod")
                {
                    canPush = CanPushToProd(user);
                }

                if (!canPush)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsInRole(IPrincipal user, IEnumerable<string> permissions)
        {
            foreach (var permission in permissions)
            {
                if (user.IsInRole(permission))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

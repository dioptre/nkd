using System.Collections.Generic;
using Orchard.Security.Permissions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Extensions;

namespace Contrib.Mod.ChangePassword
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ChangePassword = new Permission { Description="Change user password", Name = "EditUserPassword" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
                ChangePassword
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ChangePassword }
                }
            };
        }
    }
}
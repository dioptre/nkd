using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace NKD {
    public class Permissions : IPermissionProvider {
        public static readonly Permission Management = new Permission { Description = "Management", Name = "Management" };
        public static readonly Permission Lead = new Permission { Description = "Lead", Name = "Lead", ImpliedBy = new[] { Management } };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                Management,
                Lead
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {Management}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {Management}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] {Lead}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                },
            };
        }

    }
}
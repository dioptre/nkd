using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace M33.Layout
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission EditFiles = new Permission { Description = "Edit files", Name = "EditFiles" };
        

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
                EditFiles
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {EditFiles}
                },
                new PermissionStereotype {
                    Name = "Anonymous",
                    Permissions = new[] {EditFiles}
                },
                new PermissionStereotype {
                    Name = "Authenticated",
                    Permissions = new[] {EditFiles}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {EditFiles}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = new[] {EditFiles}
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] {EditFiles}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] {EditFiles}
                },
            };
        }
    }
}
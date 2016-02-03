using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
//using Orchard.Environment.State;
//using Orchard.Recipes.Events;
//using Orchard.Environment;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.Descriptor.Models;

namespace NKD.Models
{
    public class ShellTask
    {
        public string ProcessId { get; set; }
        public string TaskId { get; set; }
        public IShellContextFactory ShellContextFactory { get; set; }
        public ShellSettings ShellSettings { get; set; }
        public ShellDescriptor ShellDescriptor { get; set; }
        public Action<ContentItem> ShellAction { get; set; }
        public ContentItem ShellData { get; set; }

    }
}
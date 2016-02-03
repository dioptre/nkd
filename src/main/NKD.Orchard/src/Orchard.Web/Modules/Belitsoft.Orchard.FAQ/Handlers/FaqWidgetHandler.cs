using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Belitsoft.Orchard.Faq.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Belitsoft.Orchard.Faq.Handlers
{
    public class FaqWidgetHandler : ContentHandler
    {
        public FaqWidgetHandler(IRepository<FaqWidgetPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
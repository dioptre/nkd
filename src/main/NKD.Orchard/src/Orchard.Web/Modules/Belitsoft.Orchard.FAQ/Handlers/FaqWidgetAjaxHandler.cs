using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Belitsoft.Orchard.Faq.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Belitsoft.Orchard.Faq.Handlers
{
    public class FaqWidgetAjaxHandler : ContentHandler
    {
        public FaqWidgetAjaxHandler(IRepository<FaqWidgetAjaxPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
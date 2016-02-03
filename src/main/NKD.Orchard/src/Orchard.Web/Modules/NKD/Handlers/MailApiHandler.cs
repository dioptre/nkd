using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NKD.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace NKD.Handlers
{
    public class MailApiHandler : ContentHandler
    {
        public MailApiHandler(IRepository<MailApiPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new ActivatingFilter<MailApiPart>("MailApi"));
        }
    }

}
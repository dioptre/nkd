using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Belitsoft.Orchard.Faq.Models
{

    public class FaqWidgetPartRecord : ContentPartRecord
    {

    }

    public class FaqWidgetPart : ContentPart<FaqWidgetPartRecord>
    {

    }
}
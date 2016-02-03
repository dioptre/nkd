using System.Collections.Generic;
using Belitsoft.Orchard.Faq.Models;
using Belitsoft.Orchard.Faq.ViewModels;
using Orchard;
using Orchard.ContentManagement;

namespace Belitsoft.Orchard.Faq.Services
{
    public interface IFaqService : IDependency
    {
        IEnumerable<FaqPart> GetLastFaqs(int? count = null, int? page = null);
        int FilterByTypeId { get; set; }
        double GetCountOfPage(int count);
        IEnumerable<FaqPart> GetTypedFaqs(int faqTypeId);
        void UpdateFaqForContentItem(ContentItem item, EditFaqViewModel model);
    }
}
using System.Collections.Generic;
using Belitsoft.Orchard.Faq.Models;
using Belitsoft.Orchard.Faq.ViewModels;
using Orchard;
using Orchard.ContentManagement;

namespace Belitsoft.Orchard.Faq.Services
{
    public interface IFaqTypeService : IDependency
    {
        List<FaqTypePartRecord> GetFaqTypes();
        //void UpdateFaqForContentItem(ContentItem item, EditFaqViewModel model);
        FaqTypePartRecord GetFaqType(int id);
        bool TypeNameAlredyExists(string name);
    }


}
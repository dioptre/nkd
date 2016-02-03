using System.Collections.Generic;
using System.Linq;
using Belitsoft.Orchard.Faq.Models;
using Belitsoft.Orchard.Faq.ViewModels;
using Orchard.ContentManagement;
using Orchard.Data;

namespace Belitsoft.Orchard.Faq.Services
{
    public class FaqTypeService : IFaqTypeService
    {
        private readonly IRepository<FaqTypePartRecord> _FaqTypeRepository;

        public FaqTypeService(IRepository<FaqTypePartRecord> FaqTypeRepository)
        {
            _FaqTypeRepository = FaqTypeRepository;
        }

        public List<FaqTypePartRecord> GetFaqTypes()
        {
            return _FaqTypeRepository.Table.ToList();
        }

        public FaqTypePartRecord GetFaqType(int id)
        {
            return _FaqTypeRepository.Get(p => p.Id == id) ?? new FaqTypePartRecord();
        }

        public bool TypeNameAlredyExists(string name)
        {
            return _FaqTypeRepository.Table.Where(fp=> fp.Title == name).Count() >=2;
        }

        //public void UpdateFaqForContentItem(ContentItem item, EditFaqViewModel model)
        //{
        //    var FaqPart = item.As<FaqPart>();
        //    FaqPart.Question = model.Question;
        //    FaqPart.FaqTypeId = model.FaqType;
        //    _FaqTypeRepository.Flush();
        //}
    }
}
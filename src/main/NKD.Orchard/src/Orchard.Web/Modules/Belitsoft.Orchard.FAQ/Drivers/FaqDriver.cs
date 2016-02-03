using System.Linq;
using Belitsoft.Orchard.Faq.Models;
using Belitsoft.Orchard.Faq.Services;
using Belitsoft.Orchard.Faq.ViewModels;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Localization;

namespace Belitsoft.Orchard.Faq.Drivers
{
    public class FaqDriver : ContentPartDriver<FaqPart>
    {
        private readonly IFaqTypeService _faqTypeService;
        private readonly IContentManager _contentManager;
        private readonly IFaqService _faqService;

        private Localizer T;

        public FaqDriver(IContentManager contentManager, IFaqService faqService, IFaqTypeService faqTypeService)
        {
            _contentManager = contentManager;
            _faqTypeService = faqTypeService;
            _faqService = faqService;
        }

        protected override string Prefix
        {
            get { return "Faq"; }
        }

        protected override DriverResult Display(FaqPart part, string displayType, dynamic shapeHelper)
        {
            var faqType =
                _contentManager.Query<FaqTypePart>(VersionOptions.Published, "FaqType")
                               .Where<FaqTypePartRecord>(t => t.Id == part.FaqTypeId)
                               .List()
                               .FirstOrDefault();

            return ContentShape("Parts_Faq",
                                () => shapeHelper.Parts_Faq(
                                    Question: part.Question,
                                    FaqType: part.FaqTypeId != 0 ? _faqTypeService.GetFaqType(part.FaqTypeId).Title : string.Empty,
                                    Answer: part.ContentItem.As<BodyPart>().Text));
        }


        protected override DriverResult Editor(FaqPart part, dynamic shapeHelper)
        {
            var temp = ContentShape("Parts_Faq_Edit",
                                () => shapeHelper.EditorTemplate(
                                    TemplateName: "Parts/Faq",
                                    Model: BuildEditorViewModel(part),
                                    Prefix: Prefix));
            return temp;
        }

        //POST
        protected override DriverResult Editor(FaqPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            var model = new EditFaqViewModel();
            if (updater.TryUpdateModel(model, Prefix, null, null))
            {
                if (string.IsNullOrWhiteSpace(model.Question))
                {
                    updater.AddModelError(Prefix, T("Error"));
                }
            }
            if (part.ContentItem.Id != 0)
            {
                _faqService.UpdateFaqForContentItem(part.ContentItem, model);
            }
            
            return Editor(part, shapeHelper);
        }

        protected override void Exporting(FaqPart part, global::Orchard.ContentManagement.Handlers.ExportContentContext context)
        {
            context.Element(part.PartDefinition.Name)
                       .SetAttributeValue("FaqTypeId", part.FaqTypeId);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Question", part.Question);
        }

        protected override void Importing(FaqPart part, global::Orchard.ContentManagement.Handlers.ImportContentContext context)
        {
            part.Question = context.Attribute(part.PartDefinition.Name, "Question") ?? string.Empty;
            part.FaqTypeId = context.Attribute(part.PartDefinition.Name, "FaqTypeId") == string.Empty ? 0 : int.Parse(context.Attribute(part.PartDefinition.Name, "FaqTypeId"));
        }


        private EditFaqViewModel BuildEditorViewModel(FaqPart part)
        {
            var avm = new EditFaqViewModel
            {
                Question = part.Question,
                FaqTypes = _faqTypeService.GetFaqTypes()
            };

            if (part.FaqTypeId > 0)
            {
                avm.FaqType = part.FaqTypeId;
            }

            return avm;
        }
    }
}

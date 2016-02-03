using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using NKD.Models;

namespace NKD.Handlers {
    [UsedImplicitly]
    public class NKDSettingsPartHandler : ContentHandler {
        public NKDSettingsPartHandler(IRepository<NKDSettingsPartRecord> repository) {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<NKDSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new TemplateFilterForRecord<NKDSettingsPartRecord>("Settings", "Parts/NKD.Settings", "business"));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Business")));
        }
    }
}
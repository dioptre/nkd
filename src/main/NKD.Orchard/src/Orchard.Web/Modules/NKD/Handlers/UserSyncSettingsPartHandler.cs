using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using NKD.Models;

namespace NKD.Handlers {
    [UsedImplicitly]
    public class UserSyncSettingsPartHandler : ContentHandler
    {
        public UserSyncSettingsPartHandler(
            IRepository<UserSyncSettingsPartRecord> repository)
        {
            Filters.Add(new ActivatingFilter<UserSyncSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
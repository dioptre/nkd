using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using NKD.Models;
using Orchard;
using Orchard.Services;
using Orchard.UI.Notify;
using Orchard.Localization;



namespace NKD.Drivers
{
    [UsedImplicitly]
    public class UserSyncSettingsPartDriver : ContentPartDriver<UserSyncSettingsPart>
    {

        public UserSyncSettingsPartDriver(INotifier notifier, IOrchardServices services)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        private const string TemplateName = "UserSync.Settings";
        private readonly INotifier _notifier;

        protected override DriverResult Editor(
            UserSyncSettingsPart part, dynamic shapeHelper)
        {

            return ContentShape("UserSync_Settings",
                       () => shapeHelper.EditorTemplate(
                           TemplateName: TemplateName,
                           Model: part,
                           Prefix: Prefix));
        }

        protected override DriverResult Editor(
            UserSyncSettingsPart part, IUpdateModel updater, dynamic shapeHelper)
        {

            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(
                    T("User sync settings updated successfully"));
            }
            else
            {
                _notifier.Error(
                    T("Error during user sync settings update!"));
            }
            return Editor(part, shapeHelper);
        }
    }
}
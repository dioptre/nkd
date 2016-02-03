using Orchard.ContentManagement;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace NKD.Models {

    public class UserSyncSettingsPartRecord : ContentPartRecord
    {
        public virtual System.DateTime? NextUserSyncUtc { get; set; }

    }

    public class UserSyncSettingsPart : ContentPart<UserSyncSettingsPartRecord> 
    {
        public System.DateTime? NextUserSyncUtc
        {
            get { return Record.NextUserSyncUtc; }
            set { Record.NextUserSyncUtc = value; }
        }
    }
}
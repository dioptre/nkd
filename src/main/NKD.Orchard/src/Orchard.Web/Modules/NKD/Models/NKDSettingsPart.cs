using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement;

namespace NKD.Models {
    
    public class NKDSettingsPart : ContentPart<NKDSettingsPartRecord> {
        public string UploadAllowedFileTypeWhitelist {
            get { return Record.UploadAllowedFileTypeWhitelist; }
            set { Record.UploadAllowedFileTypeWhitelist = value; }
        }
    }
    
    public class NKDSettingsPartRecord : ContentPartRecord {
        internal const string DefaultWhitelist = "nkd zip";
        private string _whitelist = DefaultWhitelist;

        [StringLength(255)]
        public virtual string UploadAllowedFileTypeWhitelist {
            get { return _whitelist; }
            set { _whitelist = value; }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NKD.Models
{
    public interface IVersionable
    {
        Guid? VersionReferenceID { get; set; }
        string VersionClassName { get; set; }
        string VersionTableName { get; set; }
        
        int? Version { get; set; }
        Guid? VersionAntecedentID { get; set; }
        int? VersionCertainty { get; set; }
        Guid? VersionWorkflowInstanceID { get; set; }
        Guid? VersionUpdatedBy { get; set; }
        Guid? VersionDeletedBy { get; set; }
        Guid? VersionOwnerContactID { get; set; }
        Guid? VersionOwnerCompanyID { get; set; }
        DateTime VersionUpdated { get; set; }
    }
}

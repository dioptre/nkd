using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NKD.Models
{
    public interface ISecurable
    {
        string OwnerTableType { get; }
        Guid? OwnerReferenceID { get; set; }
        string OwnerEntitySystemType { get; set; }
        string OwnerField { get; set; }

        IQueryable SecurityBlacklist { get; set; }
        IQueryable SecurityWhitelist { get; set; }

        SelectList Contacts { get; set; }
        SelectList Companies { get; set; }
        SelectList Roles { get; set; }
    }
}

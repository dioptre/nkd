using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.ComponentModel;

namespace NKD.Models
{
    public interface ISecured
    {
        Guid? SecurityID { get; set; }
        bool IsBlack { get; }

        Guid? AccessorApplicationID { get; set; }
        Guid? AccessorCompanyID { get; set; }
        Guid? AccessorRoleID { get; set; }
        Guid? AccessorContactID { get; set; }
        Guid? AccessorProjectID { get; set; }

        Guid? OwnerApplicationID { get; set; }
        Guid? OwnerCompanyID { get; set; }
        Guid? OwnerContactID { get; set; }
        Guid? OwnerProjectID { get; set; }
        string OwnerEntitySystemType { get; set; }
        string OwnerTableType { get; set; }
        string OwnerField { get; set; }
        Guid? OwnerReferenceID { get; set; }

        bool CanRead { get; set; }
        bool CanCreate { get; set; }
        bool CanUpdate { get; set; }
        bool CanDelete { get; set; }

    }

    public class SecuredBasic : ISecured
    {
        public Guid? SecurityID { get; set; }
        
        [DefaultValue(true)]
        public bool IsBlack { get; set; }

        public Guid? AccessorApplicationID { get; set; }
        public Guid? AccessorCompanyID { get; set; }
        public Guid? AccessorRoleID { get; set; }
        public Guid? AccessorContactID { get; set; }
        public Guid? AccessorProjectID { get; set; }

        public Guid? OwnerApplicationID { get; set; }
        public Guid? OwnerCompanyID { get; set; }
        public Guid? OwnerContactID { get; set; }
        public Guid? OwnerProjectID { get; set; }
        public string OwnerEntitySystemType { get; set; }
        public string OwnerTableType { get; set; }
        public string OwnerField { get; set; }
        public Guid? OwnerReferenceID { get; set; }

        [DefaultValue(false)]
        public bool CanRead { get; set; }
        [DefaultValue(false)]
        public bool CanCreate { get; set; }
        [DefaultValue(false)]
        public bool CanUpdate { get; set; }
        [DefaultValue(false)]
        public bool CanDelete { get; set; }

    }

    [Flags]
    public enum ActionPermission : uint
    {
        None = 0x00,
        Read = 0x01,
        Update = 0x02,
        Create = 0x04,
        Delete = 0x08
    }
}

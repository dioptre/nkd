using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NKD.Models
{
    public interface ISecuredDetailed : ISecured
    {
        Guid? SecurityBlacklistID { get; set; }
        Guid? SecurityWhitelistID { get; set; }
        
    }
}

using System;
using System.Collections.Generic;
using NKD.Module.BusinessObjects;

namespace NKD.Services
{
    public interface IAuthority
    {
        bool IsAuthorised(bool checkLicense, Authority.ActionType action, string dataType, string tableType, string field, Guid? referenceID, Guid? applicationID, Guid? licenseID, Guid? assetID, Guid? modelID, Guid? partID, Guid? companyID, Guid? contactID, Guid? projectID, Guid? roleID);
        List<SecurityWhitelist> AuthorisedList
        {
            get;
        }
    }
}

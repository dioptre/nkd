using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NKD.Module.BusinessObjects;

namespace NKD.Services
{
    public class Authority : NKD.Services.IAuthority
    {

        [Flags]
        public enum ActionType : uint
        {
            Create = 0x01,
            Read = 0x02,
            Update = 0x04,
            Delete = 0x08
        }

        private Guid contactID;
        private Guid? userID;
        private Guid? applicationID;
        private string username;
        private Guid[] users;
        private Guid[] rootCompanies;
        private Guid[] companies;
        private Guid[] roles;
        private Guid[] applications;
        private License[] licenses;
        private LicenseAsset[] assets;
        private LicenseAssetModelPart[] parts;
        private Experience[] experiences;
        private TimeSpan expirationMaximum = new TimeSpan(1, 0, 0);

        private DateTime lastUpdated;
        public DateTime LastUpdated
        {
            get { return lastUpdated; }
            set { lastUpdated = value; }
        }
        private List<SecurityWhitelist> authorisedList = new List<SecurityWhitelist>();
        public List<SecurityWhitelist> AuthorisedList {
            get
            {
                if ((DateTime.UtcNow - lastUpdated) > expirationMaximum)
                    throw new ExpiredAuthorityException();
                return authorisedList;
            }
        }
        
        public Authority(Guid contactID, 
                         string username = null,
                         Guid? userID = null,
                         Guid? applicationID = null,
                         IEnumerable<SecurityBlacklist> blackList = null,
                         IEnumerable<SecurityWhitelist> whiteList = null,
                         IEnumerable<Guid> applications = null,
                         IEnumerable<Guid> roles = null,
                         IEnumerable<Experience> experiences = null,
                         IEnumerable<License> licenses = null,
                         IEnumerable<LicenseAsset> assets = null,
                         IEnumerable<LicenseAssetModelPart> parts = null,
                         IEnumerable<Guid> users = null,
                         IEnumerable<Guid> companies = null,
                         IEnumerable<Guid> rootCompanies = null
                        )
        {
            this.contactID = contactID;
            this.username = username;
            this.userID = userID;
            this.applicationID = applicationID;
            updateAuthority(blackList, whiteList);
            this.applications = (applications == null) ? new Guid[] { } : applications.ToArray();
            this.roles = (roles == null) ? new Guid[] { } : roles.ToArray();
            this.experiences = (experiences == null) ? new Experience[] { } : experiences.ToArray();
            this.licenses = (licenses == null) ? new License[] { } : licenses.ToArray();
            this.assets = (assets == null) ? new LicenseAsset[] { } : assets.ToArray();
            this.parts = (parts == null) ? new LicenseAssetModelPart[] { } : parts.ToArray();
            this.users = (users == null) ? new Guid[] { } : users.ToArray();
            this.companies = (companies == null) ? new Guid[] { } : companies.ToArray();
            this.rootCompanies = (rootCompanies == null) ? new Guid[] { } : rootCompanies.ToArray();
            this.lastUpdated = DateTime.UtcNow;
        }

        private void updateAuthority(
                            IEnumerable<SecurityBlacklist> blackList,
                            IEnumerable<SecurityWhitelist> whiteList)
        {
            if (username == "admin")
                return;
            //I can change what I own - this can be taken back in blacklist
            authorisedList.Add(new SecurityWhitelist { OwnerContactID = contactID, CanCreate = true, CanDelete = true, CanRead = true, CanUpdate = true });
            //NOTHING ELSE IS GIVEN            
            blackList = blackList ?? new SecurityBlacklist[] { };
            whiteList = whiteList ?? new SecurityWhitelist[] { };
            //update authorisedList
            //Integrate now to the existing white-list
            //Simplify
            //Then trim from blacklist
            throw new NotImplementedException();
        }

        public bool IsAuthorised(bool checkLicense, 
            ActionType action, 
            string dataType, 
            string tableType, 
            string field, 
            Guid? referenceID, 
            Guid? applicationID, 
            Guid? licenseID, //Chcek license elsewhere too in binary form, optional implementation for 3rd party modules
            Guid? assetID,
            Guid? modelID,
            Guid? partID,
            Guid? companyID, 
            Guid? contactID, 
            Guid? projectID, 
            Guid? roleID)
        {
            if (username == "admin")
                return true;
            //Force recalculation of user's rights - internal limit
            if ((DateTime.UtcNow - lastUpdated) > expirationMaximum)
                throw new ExpiredAuthorityException();
            throw new NotImplementedException();
            //Check request against authorisedList
            //Note application is linked to company through license - license management is done here too
            // public bool IsAuthorised(bool checkLicense, 
            //ActionType action, 
            //string dataType, 
            //string tableType, 
            //string field, 
            //Guid? referenceID, 
            //Guid? applicationID, 
            //Guid? licenseID, //Chcek license elsewhere too in binary form, optional implementation for 3rd party modules
            //Guid? assetID,
            //Guid? modelID,
            //Guid? partID,
            //Guid? companyID, 
            //Guid? contactID, 
            //Guid? projectID, 
            //Guid? roleID
            return false;
        }


    }

    [Serializable()]
    public class AuthorityException : Exception
    {
        public AuthorityException() : base() {}
        public AuthorityException(string message) : base(message) { }
        public AuthorityException(string message, System.Exception inner) : base(message, inner) { }
        protected AuthorityException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
    }

    [Serializable()]
    public class ExpiredAuthorityException : AuthorityException
    {
        public ExpiredAuthorityException() : base() { }
        public ExpiredAuthorityException(string message) : base(message) { }
        public ExpiredAuthorityException(string message, System.Exception inner) : base(message, inner) { }
        protected ExpiredAuthorityException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
    }
}
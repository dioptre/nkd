using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace NKD.ViewModels
{

    [JsonObject]
    public class ContactViewModel 
    {

        [JsonIgnore]
        public Guid? ContactID { get; set; }
        public Guid? id { get { return ContactID; } set { ContactID = value; } }
        public Guid? UserID { get; set; }
        public string UserName { get; set; }
        public string ContactName { get; set; }
        public IEnumerable<LicenseViewModel> Licenses { get; set; } 
        public SelectListItem[] Companies { get; set; }
        public Guid? CurrentCompanyID { get; set; }
        public string CurrentCompany { get; set; }
        public bool IsSubscriber { get; set; }
        public bool IsPartner { get; set; }
        public string[] Roles { get; set; }
    }

    public class LicenseViewModel
    {
        public Guid? LicenseID { get; set; }
        public DateTime? Expiry { get; set; }
        public Guid? ModelID { get; set; }
        public string ModelName { get; set; }
        public string ModelRestrictions { get; set; }
        public Guid? PartID { get; set; }
        public string PartName { get; set; }
        public string PartRestrictions { get; set; }
    }

}
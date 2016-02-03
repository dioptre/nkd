using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NKD.Models
{

    public class VerifyMobileModel : IVerify
    {
        public Guid VerificationID { get; set; }
        public string Mobile { get; set; }
        public string VerificationCode { get; set; }
        public string TableType { get; set; }
        public Guid ReferenceID { get; set; }
        public Guid ContactID { get; set; }
        public string ReferenceName { get; set; }
        public DateTime? Sent { get; set; }
        public DateTime? Verified { get; set; }
    }

    public interface IVerify
    {
        Guid VerificationID { get; set; }
        string Mobile { get; set; }
        string VerificationCode { get; set; }
        string TableType { get; set; }
        Guid ReferenceID { get; set; }
        Guid ContactID { get; set; }
        string ReferenceName { get; set; }
        DateTime? Sent { get; set; }
        DateTime? Verified { get; set; }
    }
}
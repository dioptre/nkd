using Orchard.ContentManagement;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using System.Linq;
using System;

namespace NKD.Models {

    public class MailApiPartRecord : ContentPartRecord
    {
        public virtual string ApiMethod { get; set; }
        public virtual string ApiKey { get; set; }
        [StringLengthMax]
        public virtual string ApiValue { get; set; }
        [StringLengthMax]
        public virtual string Json { get; set; }
        public virtual int Status { get; set; }
        public virtual DateTime? Processed { get; set; }
        public virtual DateTime? Completed { get; set; }
    }

    public class MailApiPart : ContentPart<MailApiPartRecord>
    {

        public string ApiMethod { get { return Record.ApiMethod; } set { Record.ApiMethod = value; } }
        public string ApiKey { get { return Record.ApiKey; } set { Record.ApiKey = value; } }
        public string ApiValue { get { return Record.ApiValue; } set { Record.ApiValue = value; } }
        public string Json { get { return Record.Json;} set { Record.Json = value; }}
        public int Status { get { return Record.Status; } set { Record.Status = value; } }
        public DateTime? Processed { get { return Record.Processed; } set { Record.Processed = value; } }
        public DateTime? Completed { get { return Record.Completed; } set { Record.Completed = value; } }
       
    }

    [Flags]
    public enum MailApiStatus : int
    {
        Unknown = 0x0,
        Completed = 0x01
    }


}
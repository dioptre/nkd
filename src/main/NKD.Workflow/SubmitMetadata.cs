using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Activities.Persistence;
using System.Xml.Linq;

namespace NKD.Workflow
{

    public partial class SubmitMetadata : NativeActivity
    {
        public OutArgument<string> SubmittedIsCompleted { get; set; }
        public OutArgument<string> SubmittedReferenceClass { get; set; }
        public OutArgument<string> SubmittedReferenceTable { get; set; }
        public OutArgument<string> SubmittedReferenceID { get; set; }
        public OutArgument<string> SubmittedContactID { get; set; }
        public OutArgument<string> SubmittedCompanyID { get; set; }
        public OutArgument<string> SubmittedRecordState { get; set; }
        [RequiredArgument]
        public InArgument<string> BookmarkName { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.AddDefaultExtensionProvider<Helpers.WorkflowInstanceExtensionHelper>(() => new Helpers.WorkflowInstanceExtensionHelper());
        }

        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        protected override void Execute(NativeActivityContext context)
        {
            var bookmark = context.CreateBookmark(BookmarkName.Get(context), BookmarkResumed);
            var extension = context.GetExtension<Helpers.WorkflowInstanceExtensionHelper>();
            //extension.WaitSome(bookmark);

        }


        private void BookmarkResumed(NativeActivityContext context, Bookmark bookmark, object value)
        {
            var dict = (Dictionary<string,object>)value;
            object company = null, contact = null, recordState = null, isCompleted = null, referenceClass = null, referenceTable = null, referenceID = null;
            if (dict.TryGetValue("CompanyID", out company))
            {
                var temp = (string)company;
                this.SubmittedCompanyID.Set(context, temp);
                context.GetExtension<MetadataExtension>().CompanyID = temp;
            }
            if (dict.TryGetValue("ContactID", out contact))
            {
                var temp = (string)contact;
                this.SubmittedContactID.Set(context, temp);
                context.GetExtension<MetadataExtension>().ContactID = temp;
            }
            if (dict.TryGetValue("RecordState", out recordState))
            {
                this.SubmittedRecordState.Set(context, (string)dict["RecordState"]);
                context.GetExtension<MetadataExtension>().RecordState = (string)dict["RecordState"];                
            }
            if (dict.TryGetValue("IsCompleted", out isCompleted))
            {
                var temp = (string)isCompleted;
                this.SubmittedRecordState.Set(context, temp);
                context.GetExtension<MetadataExtension>().IsCompleted = temp;
            }
            if (dict.TryGetValue("ReferenceClass", out referenceClass))
            {
                var temp = (string)referenceClass;
                this.SubmittedRecordState.Set(context, temp);
                context.GetExtension<MetadataExtension>().ReferenceClass = temp;
            }
            if (dict.TryGetValue("ReferenceTable", out referenceTable))
            {
                var temp = (string)referenceTable;
                this.SubmittedRecordState.Set(context, temp);
                context.GetExtension<MetadataExtension>().ReferenceTable = temp;
            }
            if (dict.TryGetValue("ReferenceID", out referenceID))
            {
                var temp = (string)referenceID;
                this.SubmittedRecordState.Set(context, temp);
                context.GetExtension<MetadataExtension>().ReferenceID = temp;
            }



            
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debug.WriteLine("Bookmark resumed with '{0}'.", dict);
            }
            
        }
    }

    public class MetadataExtension : PersistenceParticipant
    {
        public static readonly XNamespace xNS = XNamespace.Get("http://nkd.org/Metadata");
        public string IsCompleted { get; set; }
        public string ReferenceClass { get; set; }
        public string ReferenceTable { get; set; }
        public string ReferenceID { get; set; }
        public string CompanyID { get; set; }
        public string ContactID { get; set; }
        public string RecordState { get; set; }

        protected override void CollectValues(out IDictionary<XName, object> readWriteValues, out IDictionary<XName, object> writeOnlyValues)
        {
            readWriteValues = new Dictionary<XName, object>();
            readWriteValues.Add(xNS.GetName("CompanyID"), this.CompanyID);
            readWriteValues.Add(xNS.GetName("ContactID"), this.ContactID);
            readWriteValues.Add(xNS.GetName("RecordState"), this.RecordState);
            readWriteValues.Add(xNS.GetName("IsCompleted"), this.IsCompleted);
            readWriteValues.Add(xNS.GetName("ReferenceClass"), this.ReferenceClass);
            readWriteValues.Add(xNS.GetName("ReferenceTable"), this.ReferenceTable);
            readWriteValues.Add(xNS.GetName("ReferenceID"), this.ReferenceID);

            writeOnlyValues = null;
        }
    
    }


}

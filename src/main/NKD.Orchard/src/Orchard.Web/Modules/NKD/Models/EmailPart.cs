using Orchard.ContentManagement;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using System.Linq;

namespace NKD.Models {

    public class EmailPartRecord : ContentPartRecord, IMessage
    {
        [StringLengthMax]
        public virtual string Recipients { get; set; }
        public virtual string Subject { get; set; }
        public virtual string Body { get; set; }
        public virtual bool Retry { get; set; }
        public virtual bool ForwardSupport { get; set; }
        public virtual bool HideRecipients { get; set; }
        public virtual string FromAddress { get; set; }
        public virtual string FromName { get; set; }
        public virtual System.DateTime? Processed { get; set; }
        public virtual System.DateTime? Completed { get; set; }
    }

    public class EmailPart : ContentPart<EmailPartRecord>, IMessage
    {
        private string _recipients { get; set; }
        private string _subject { get; set; }
        private string _body { get; set; }
        private bool _retry { get; set; }
        private bool _forwardSupport { get; set; }
        private string _fromAddress { get; set; }
        private string _fromName { get; set; }
        private System.DateTime? _processed { get; set; }
        private System.DateTime? _completed { get; set; }
        private bool _hideRecipients { get; set; }



        public virtual bool HideRecipients
        {
            get
            {
                if (Record != null)
                    return Record.HideRecipients;
                return _hideRecipients;
            }
            set
            {
                if (Record != null)
                    Record.HideRecipients = value;
                _hideRecipients = value;
            }
        }

        public virtual bool ForwardSupport
        {
            get
            {
                if (Record != null)
                    return Record.ForwardSupport;
                return _forwardSupport;
            }
            set
            {
                if (Record != null)
                    Record.ForwardSupport = value;
                _forwardSupport = value;
            }
        }
        public virtual string FromName
        {
            get
            {
                if (Record != null)
                    return Record.FromName;
                return _fromName;
            }
            set
            {
                if (Record != null)
                    Record.FromName = value;
                _fromName = value;
            }
        }

        public virtual string FromAddress
        {
            get
            {
                if (Record != null)
                    return Record.FromAddress;
                return _fromAddress;
            }
            set
            {
                if (Record != null)
                    Record.FromAddress = value;
                _fromAddress = value;
            }
        }


        public bool Retry
        {
            get
            {
                if (Record != null)
                    return Record.Retry;
                return _retry;
            }
            set
            {
                if (Record != null)
                    Record.Retry = value;
                _retry = value;
            }
        }

        public System.DateTime? Completed
        {
            get
            {
                if (Record != null)
                    return Record.Completed;
                return _completed;
            }
            set
            {
                if (Record != null)
                    Record.Completed = value;
                _completed = value;
            }
        }

        public System.DateTime? Processed
        {
            get
            {
                if (Record != null)
                    return Record.Processed;
                return _processed;
            }
            set
            {
                if (Record != null)
                    Record.Processed = value;
                _processed = value;
            }
        }

        public string Recipients
        {
            get
            {
                if (Record != null)
                    return Record.Recipients;
                return _recipients;
            }
            set
            {
                if (Record != null)
                    Record.Recipients = value;
                _recipients = value;
            }
        }

        public string Subject
        {
            get
            {
                if (Record != null)
                    return Record.Subject;
                return _subject;
            }
            set
            {
                if (Record != null)
                    Record.Subject = value;
                _subject = value;
            }
        }

        public string Body
        {
            get
            {
                if (Record != null)
                    return Record.Body;
                return _body;
            }
            set
            {
                if (Record != null)
                    Record.Body = value;
                _body = value;
            }
        }


        public EmailPart()
        { }

    }


}
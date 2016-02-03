namespace Orchard.Email.Models {
    public class EmailMessage {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Recipients { get; set; }
        public string From { get; set; }
        public string FromName { get; set; }
        public bool? HideRecipients { get; set; }
    }
}
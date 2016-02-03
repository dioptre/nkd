//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using Orchard.Messaging.Events;
//using Orchard.Messaging.Models;

//namespace NKD.Events
//{
//    public class EmailMessageHandler : IMessageEventHandler
//    {
//        public const string DEFAULT_NKD_EMAIL_TYPE = "NKD-Services-UserNotification";
        
//        public void Sending(MessageContext context)
//        {
//            if (context.MessagePrepared)
//                return;
//            switch (context.Type)
//            {
//                case DEFAULT_NKD_EMAIL_TYPE:
//                    context.MailMessage.Subject = context.Properties["Subject"];
//                    context.MailMessage.Body = context.Properties["Body"];
//                    if (context.Properties.ContainsKey("From"))
//                    {
//                        string fromName;
//                        if (context.Properties.TryGetValue("FromName", out fromName))
//                            context.MailMessage.From = new System.Net.Mail.MailAddress(context.Properties["From"], fromName);
//                        else
//                            context.MailMessage.From = new System.Net.Mail.MailAddress(context.Properties["From"]);
//                        context.MailMessage.ReplyToList.Add(context.MailMessage.From.Address);
//                    }
//                    if (context.Properties.ContainsKey("HideRecipients"))
//                    {
//                        for (int i = context.MailMessage.To.Count - 1; i > -1; i--)
//                        {
//                            context.MailMessage.Bcc.Add(context.MailMessage.To[i]);
//                            context.MailMessage.To.RemoveAt(i);
//                        }
//                        for (int i = context.MailMessage.CC.Count - 1; i > -1; i--)
//                        {
//                            context.MailMessage.Bcc.Add(context.MailMessage.CC[i]);
//                            context.MailMessage.CC.RemoveAt(i);
//                        }
//                        context.MailMessage.To.Add(context.MailMessage.From);
//                    }
//                    context.MessagePrepared = true;
//                    break;
//            }
//        }

//        //we don't care about this right now 
//        public void Sent(MessageContext context) { }

//    }
//}
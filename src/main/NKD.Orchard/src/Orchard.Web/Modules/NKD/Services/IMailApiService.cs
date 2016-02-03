using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Orchard;
using NKD.Module.BusinessObjects;
using NKD.Models;
using System.ServiceModel;

namespace NKD.Services
{
    [ServiceContract]
    public interface IMailApiService : IDependency
    {
        [OperationContract]
        void AddMemberToMailingList(Contact c, string mailingListName);

        [OperationContract]
        void AddEmailAlias(string mailUsername, string alias);

        [OperationContract]
        void RemoveEmailAlias(string mailUsername, string alias);

        [OperationContract]
        void ProcessApiRequest(MailApiPart request);

        [OperationContract]
        void ProcessApiRequestAsync(MailApiCall apiMethod, string apiKey, string apiValue = null, string json = null, int status = 0, DateTime? processed = default(DateTime?), DateTime? completed = default(DateTime?), bool shortLived = true);        

    }
    
    public enum MailApiCall
    {
        AliasRemove,
        AliasAdd,
        MailingListAdd,
        MailingListRemove
    }
}
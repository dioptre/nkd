using System;
using System.ServiceModel.Web;

namespace Gallery.Infrastructure.Interfaces
{
    public interface IWebFaultExceptionCreator
    {
        WebFaultException<string> CreateWebFaultException(Exception originalException, string errorMessagePrefix);
    }
}
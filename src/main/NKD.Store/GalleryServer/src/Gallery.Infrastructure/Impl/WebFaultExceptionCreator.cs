using System;
using System.Net;
using System.ServiceModel.Web;
using Gallery.Core.Exceptions;
using Gallery.Infrastructure.Interfaces;

namespace Gallery.Infrastructure.Impl
{
    public class WebFaultExceptionCreator : IWebFaultExceptionCreator
    {
        public WebFaultException<string> CreateWebFaultException(Exception originalException, string errorMessagePrefix)
        {
            HttpStatusCode statusCodeToThrow;
            if (originalException is WebFaultException<string>)
            {
                return (WebFaultException<string>)originalException;
            }
            if (originalException is WebFaultException)
            {
                statusCodeToThrow = ((WebFaultException)originalException).StatusCode;
                return new WebFaultException<string>(string.Format("{0}.", errorMessagePrefix), statusCodeToThrow);
            }
            if (originalException is ObjectDoesNotExistException)
            {
                statusCodeToThrow = HttpStatusCode.NotFound;
            }
            else if (originalException is PackageAuthorizationException)
            {
                statusCodeToThrow = HttpStatusCode.Unauthorized;
            }
            else
            {
                statusCodeToThrow = HttpStatusCode.InternalServerError;
            }
            return new WebFaultException<string>(string.Format("{0}: {1}", errorMessagePrefix, originalException.Message), statusCodeToThrow);
        }
    }
}
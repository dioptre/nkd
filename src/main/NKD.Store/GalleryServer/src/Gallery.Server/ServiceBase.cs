using System;
using System.IO;
using System.ServiceModel;
using Gallery.Core.Exceptions;
using Gallery.Infrastructure.Interfaces;
using Ninject.Extensions.Logging;

namespace Gallery.Server
{
    public abstract class ServiceBase
    {
        private readonly IWebFaultExceptionCreator _webFaultExceptionCreator;
        private readonly ILogger _logger;

        protected ServiceBase(IWebFaultExceptionCreator webFaultExceptionCreator, ILogger logger)
        {
            _webFaultExceptionCreator = webFaultExceptionCreator;
            _logger = logger;
        }

        protected void ExecuteAction(Action actionToExecute, string errorMessagePrefix)
        {
            try
            {
                actionToExecute();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "{0}.", errorMessagePrefix);
                throw _webFaultExceptionCreator.CreateWebFaultException(ex, errorMessagePrefix);
            }
        }

        protected T ExecuteAction<T>(Func<T> actionToExecute, string errorMessagePrefix)
        {
            try
            {
                return actionToExecute();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "{0}.", errorMessagePrefix);
                Exception exceptionToThrow = ex;
                if (ex.InnerException is CommunicationException && ex.InnerException.InnerException is QuotaExceededException)
                {
                    exceptionToThrow = new PackageTooBigException();
                }
                throw _webFaultExceptionCreator.CreateWebFaultException(exceptionToThrow, errorMessagePrefix);
            }
        }

        protected void ValidateInputs(Action validate)
        {
            try
            {
                validate();
            }
            catch (Exception ex)
            {
                const string errorMessagePrefix = "An error occurred when attempting to validate inputs";
                _logger.Error(ex, "{0}.", errorMessagePrefix);
                throw _webFaultExceptionCreator.CreateWebFaultException(ex, errorMessagePrefix);
            }
        }
    }
}
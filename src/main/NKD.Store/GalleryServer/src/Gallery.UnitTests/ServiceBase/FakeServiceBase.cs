using System;
using Gallery.Infrastructure.Interfaces;
using Moq;
using Ninject.Extensions.Logging;

namespace Gallery.UnitTests.ServiceBase
{
    public class FakeServiceBase : Server.ServiceBase
    {
        public FakeServiceBase(IWebFaultExceptionCreator webFaultExceptionCreator, ILogger logger)
            : base(webFaultExceptionCreator, logger)
        { }

        public void ExecuteAction(Action actionToExecute)
        {
            ExecuteAction(actionToExecute, string.Empty);
        }

        public T ExecuteAction<T>(Func<T> actionToExecute)
        {
            return ExecuteAction(actionToExecute, string.Empty);
        }
    }
}
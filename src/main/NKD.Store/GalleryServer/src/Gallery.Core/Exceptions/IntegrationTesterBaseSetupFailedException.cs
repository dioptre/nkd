using System;

namespace Gallery.Core.Exceptions
{
    public class IntegrationTesterBaseSetupFailedException : Exception
    {
        public IntegrationTesterBaseSetupFailedException(string reason, Exception originalException)
            : base(string.Format("IntegrationTesterBase setup failed: {0}.", reason), originalException)
        { }
    }
}
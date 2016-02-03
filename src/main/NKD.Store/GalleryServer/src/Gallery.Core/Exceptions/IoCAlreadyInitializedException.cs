using System;

namespace Gallery.Core.Exceptions
{
    public class IoCAlreadyInitializedException : Exception
    {
        public IoCAlreadyInitializedException()
            : base("An attempt was made to initialize the IoC Container after it has already been initialized.")
        { }
    }
}

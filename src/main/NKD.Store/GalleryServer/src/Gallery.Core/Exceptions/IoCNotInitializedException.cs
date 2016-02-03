using System;

namespace Gallery.Core.Exceptions
{
    public class IoCNotInitializedException : Exception
    {
        public IoCNotInitializedException()
            : base("An attempt was made to use the IoC Container before it was initialized with a resolver.")
        { }
    }
}

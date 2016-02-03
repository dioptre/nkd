using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;

namespace Gallery.Core
{
    public static class IoC
    {
        private static IDependencyResolver _resolver;
        public static IDependencyResolver Resolver
        {
            get
            {
                if (_resolver == null)
                {
                    throw new IoCNotInitializedException();
                }
                return _resolver;
            }
            set
            {
                if (_resolver != null)
                {
                    throw new IoCAlreadyInitializedException();
                }
                _resolver = value;
            }
        }
    }
}

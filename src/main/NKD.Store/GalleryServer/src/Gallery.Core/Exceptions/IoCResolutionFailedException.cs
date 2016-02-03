using System;

namespace Gallery.Core.Exceptions
{
    public class IoCResolutionFailedException : Exception
    {
        public IoCResolutionFailedException(Type baseType, Exception innerException)
            : base(string.Format("Resolution failed for type {0}.", baseType.Name), innerException)
        {
            BaseType = baseType;
        }

        public IoCResolutionFailedException(Type baseType, string key, Exception innerException)
            : base(string.Format("Resolution failed for type {0} with key {1}.", baseType.Name, key), innerException)
        {
            BaseType = baseType;
            Key = key;
        }

        private Type BaseType { get; set; }
        private string Key { get; set; }
    }
}
using System;

namespace Gallery.Core.Exceptions
{
    public class InvalidUserKeyException: Exception
    {
        public InvalidUserKeyException(string key)
            : base(string.Format("The user api key '{0}' is not valid.", key))
        { }
    }
}
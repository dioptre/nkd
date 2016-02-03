using System;

namespace Gallery.Core.Exceptions
{
    public class MissingAppSettingException : Exception
    {
        public MissingAppSettingException(string key)
            : base(string.Format("The app setting key '{0}' was not found.", key))
        { }
    }
}
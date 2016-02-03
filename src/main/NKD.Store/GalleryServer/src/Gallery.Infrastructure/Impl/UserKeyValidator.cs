using System;
using Gallery.Infrastructure.Interfaces;

namespace Gallery.Infrastructure.Impl
{
    public class UserKeyValidator : IUserKeyValidator
    {
        public bool IsValidUserKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            Guid checkedKey;
            return Guid.TryParse(key, out checkedKey);
        }
    }
}
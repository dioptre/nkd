using System;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class SystemGuid : IGuid
    {
        public Guid NewGuid()
        {
            return Guid.NewGuid();
        }
    }
}
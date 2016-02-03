using System;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class SystemDateTime : IDateTime
    {
        public DateTime UtcNow { get { return DateTime.UtcNow; } }
    }
}
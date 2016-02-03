using System;

namespace Gallery.Core.Interfaces
{
    public interface IDateTime
    {
        DateTime UtcNow { get; }
    }
}
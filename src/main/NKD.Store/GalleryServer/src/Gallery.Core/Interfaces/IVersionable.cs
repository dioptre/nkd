using System;

namespace Gallery.Core.Interfaces
{
    public interface IVersionable
    {
        string Id { get; set; }
        string Version { get; set; }
        DateTime? Published { get; set; }

        /// <summary>
        /// This actually corresponds to whether or not the version is recommended, not to whether or not it is the latest.
        /// </summary>
        bool IsLatestVersion { get; set; }
    }
}
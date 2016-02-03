using System;

namespace Gallery.Core.Interfaces
{
    public interface IGalleryUriValidator
    {
        bool IsValidUri(string uri, UriKind allowedUriKind = UriKind.RelativeOrAbsolute);
    }
}
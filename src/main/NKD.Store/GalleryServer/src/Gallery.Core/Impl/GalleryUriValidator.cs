using System;
using System.Collections.Generic;
using System.Linq;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class GalleryUriValidator : IGalleryUriValidator
    {
        public bool IsValidUri(string uri, UriKind allowedUriKind)
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                return true;
            }
            Uri checkedUri;
            if (!Uri.TryCreate(uri, allowedUriKind, out checkedUri))
            {
                return false;
            }
            if (checkedUri.IsAbsoluteUri)
            {
                IEnumerable<string> validSchemes = new[] { "http", "https" };
                return validSchemes.Any(s => s == checkedUri.Scheme);
            }
            return true;
        }
    }
}
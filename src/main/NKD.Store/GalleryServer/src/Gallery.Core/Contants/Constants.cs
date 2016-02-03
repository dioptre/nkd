using System;

namespace Gallery.Core.Contants
{
    public static class Constants
    {
        private static DateTime _unpublishedDate = new DateTime(1900, 1, 1);

        //Can't make this an actual constant, so faking it with a static accessor
        public static DateTime UnpublishedDate { get { return _unpublishedDate; }}
    }
}
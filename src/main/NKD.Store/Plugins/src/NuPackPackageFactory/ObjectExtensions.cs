namespace Gallery.Plugins.NuPackPackageFactory
{
    public static class ObjectExtensions
    {
        public static string ToSafeString(this object nullableObject)
        {
            return nullableObject != null ? nullableObject.ToString() : string.Empty;
        }
    }
}
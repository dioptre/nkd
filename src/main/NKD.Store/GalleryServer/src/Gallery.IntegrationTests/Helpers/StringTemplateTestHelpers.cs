namespace Gallery.IntegrationTests.Helpers
{
    internal static class StringTemplateTestHelpers
    {
        internal static string RemoveTemplateTokens(this string template)
        {
            return template.Replace("{", "").Replace("}", "");
        }
    }
}
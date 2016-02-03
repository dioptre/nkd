using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Orchard.Themes.Services;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;

namespace M33.Layout.Services
{
    /// <summary>
    /// Populates layout names automatically by inspecting current theme
    /// </summary>
    public class ThemeLayoutAlternatesProvider : ILayoutAlternatesProvider
    {
        private readonly ISiteThemeService _siteThemeService;
        private readonly IExtensionManager _extensionManager;

        public ThemeLayoutAlternatesProvider(
            ISiteThemeService siteThemeService,
            IExtensionManager extensionManager
            )
        {
            _siteThemeService = siteThemeService;
            _extensionManager = extensionManager;
        }

        public IEnumerable<string> GetLayouts()
        {
            var theme = _siteThemeService.GetSiteTheme();

            return ExtractLayoutNames(theme);
        }

        private IEnumerable<string> ExtractLayoutNames(ExtensionDescriptor theme)
        {
            var views = Directory.EnumerateFiles(HttpContext.Current.Server.MapPath(
                        string.Format("{0}/{1}/Views/", theme.Location, theme.Name.Replace(" ", ""))), "*.cshtml").Select(template =>
                        {
                            var f = new FileInfo(template);

                            if (f.Name.StartsWith("Layout-"))
                            {
                                string fname = f.Name.Replace("Layout-", "")
                                               .Replace("-", " ")
                                               .Replace(f.Extension, "");

                                if (fname.Length > 0)
                                    return fname;
                            }
                            return null;
                        }).Where(n => n != null);

            // Traverse base themes
            if (!String.IsNullOrWhiteSpace(theme.BaseTheme))
            {
                var baseTheme = _extensionManager.GetExtension(theme.BaseTheme);
                // Concat not Union since Distinct is enforce in service anyway
                views = views.Concat(ExtractLayoutNames(baseTheme));
            }
            return views;
        }

    }
}

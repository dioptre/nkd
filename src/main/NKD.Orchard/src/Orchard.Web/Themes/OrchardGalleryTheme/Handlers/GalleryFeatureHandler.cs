using Contrib.Taxonomies.Services;
using Orchard.ContentManagement;
using Orchard.Environment;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Gallery {
    public class GalleryFeatureHandler : IFeatureEventHandler {
        private readonly ITaxonomyService _taxonomyService;
        private readonly IOrchardServices _services;

        public GalleryFeatureHandler(ITaxonomyService taxonomyService, IOrchardServices services) {
            _taxonomyService = taxonomyService;
            _services = services;
        }

        public void Installing(Feature feature) {
            
        }

        public void Installed(Feature feature) {
            
        }

        public void Enabling(Feature feature) {
            
        }

        public void Enabled(Feature feature) {
            const string taxonomyName = "Package Types";
            var taxonomy = _taxonomyService.GetTaxonomyByName(taxonomyName);

            if (taxonomy != null) {
                var moduleTerm = _taxonomyService.GetTermByName(taxonomy.Id, "Module");
                if (moduleTerm == null) {
                    moduleTerm = _taxonomyService.NewTerm(taxonomy);
                    moduleTerm.Name = "Module";
                    moduleTerm.Slug = "Modules";
                    moduleTerm.Container = taxonomy.ContentItem;
                    _services.ContentManager.Create(moduleTerm, VersionOptions.Published);
                }

                var themeTerm = _taxonomyService.GetTermByName(taxonomy.Id, "Theme");
                if (themeTerm == null) {
                    themeTerm = _taxonomyService.NewTerm(taxonomy);
                    themeTerm.Name = "Theme";
                    themeTerm.Slug = "Themes";
                    themeTerm.Container = taxonomy.ContentItem;
                    _services.ContentManager.Create(themeTerm, VersionOptions.Published);
                }
            }
        }

        public void Disabling(Feature feature) {
        }

        public void Disabled(Feature feature) {
        }

        public void Uninstalling(Feature feature) {
        }

        public void Uninstalled(Feature feature) {
        }
    }
}
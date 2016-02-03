using Orchard.UI.Resources;

namespace NKD {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            builder.Add().DefineStyle("NKDAdmin").SetUrl("nkd-admin.css");
        }
    }
}

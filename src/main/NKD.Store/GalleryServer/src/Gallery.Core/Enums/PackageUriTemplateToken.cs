namespace Gallery.Core.Enums
{
    public sealed class PackageUriTemplateToken
    {
        public static readonly PackageUriTemplateToken PackageId = new PackageUriTemplateToken("PackageId");
        public static readonly PackageUriTemplateToken PackageVersion = new PackageUriTemplateToken("PackageVersion");
        public static readonly PackageUriTemplateToken PackageTypeSlug = new PackageUriTemplateToken("PackageTypeSlug");
        public static readonly PackageUriTemplateToken PackageSlug = new PackageUriTemplateToken("PackageSlug");

        private const string TOKEN_PREFIX = "{";
        private const string TOKEN_SUFFIX = "}";

        private readonly string _token;
        public string Token { get { return string.Format("{0}{1}{2}", TOKEN_PREFIX, _token, TOKEN_SUFFIX); } }

        private PackageUriTemplateToken(string token)
        {
            _token = token;
        }

        public static implicit operator string(PackageUriTemplateToken token)
        {
            return token.ToString();
        }

        public override string ToString()
        {
            return Token;
        }
    }
}
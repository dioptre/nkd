using Gallery.Core.Enums;
using Gallery.Core.Interfaces;

namespace Gallery.DependencyResolution
{
    internal class HashEncodingTypeService
    {
        private readonly IConfigSettings _configSettings;

        public HashEncodingTypeService(IConfigSettings configSettings)
        {
            _configSettings = configSettings;
        }

        public HashEncodingType GetHashEncodingTypeToUse()
        {
            return HashEncodingType.FromName(_configSettings.HashEncodingType);
        }
    }
}
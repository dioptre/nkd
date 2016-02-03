using System.Configuration;
using Gallery.Core.Exceptions;
using Gallery.Core.Interfaces;

namespace Gallery.Core.Impl
{
    public class SystemConfigurationManager : IConfigurationManager
    {
        public string GetString(string key)
        {
            string setting = ConfigurationManager.AppSettings[key];
            if (setting == null)
            {
                throw new MissingAppSettingException(key);
            }
            return setting;
        }

        public int GetInt(string key)
        {
            int intProperty;
            string intPropertyAsString = GetString(key);
            if (!int.TryParse(intPropertyAsString, out intProperty))
            {
                throw new InvalidAppSettingException(key, intPropertyAsString);
            }
            return intProperty;
        }

        public bool GetBool(string key)
        {
            bool boolProperty;
            string boolPropertyAsString = GetString(key);
            if (!bool.TryParse(boolPropertyAsString, out boolProperty))
            {
                throw new InvalidAppSettingException(key, boolPropertyAsString);
            }
            return boolProperty;
        }

        public ConnectionStringSettings GetConnectionStringSettings(string key)
        {
            var settings = ConfigurationManager.ConnectionStrings[key];
            if (settings == null)
            {
                throw new MissingConnectionStringSettingsException(key);
            }
            return settings;
        }
    }
}
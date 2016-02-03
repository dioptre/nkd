using System.Configuration;

namespace Gallery.Core.Interfaces
{
    public interface IConfigurationManager
    {
        string GetString(string key);
        int GetInt(string key);
        bool GetBool(string key);
        ConnectionStringSettings GetConnectionStringSettings(string key);
    }
}
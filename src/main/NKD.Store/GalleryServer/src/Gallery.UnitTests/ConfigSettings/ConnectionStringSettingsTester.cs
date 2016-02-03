using System.Configuration;
using NUnit.Framework;

namespace Gallery.UnitTests.ConfigSettings
{
    public class ConnectionStringSettingsTester : ConfigSettingsTester
    {
        [Test]
        public void ShouldReturnWhatManagerReturns()
        {
            var expectedSettings = new ConnectionStringSettings();
            const string key = "key";
            MockedConfigurationManager.Setup(cm => cm.GetConnectionStringSettings(key)).Returns(expectedSettings);

            ConnectionStringSettings connectionStringSettings = ConfigSettings.ConnectionStringSettings(key);

            Assert.AreEqual(expectedSettings, connectionStringSettings, "Settings from manager should have been returned.");
        }
    }
}
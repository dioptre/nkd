using Gallery.Core.Interfaces;
using Moq;
using NUnit.Framework;

namespace Gallery.UnitTests.ConfigSettings
{
    [TestFixture]
    public abstract class ConfigSettingsTester
    {
        protected IConfigSettings ConfigSettings;

        protected Mock<IConfigurationManager> MockedConfigurationManager;
        protected Mock<IHttpRuntime> MockedHttpRuntime;

        [SetUp]
        public void SetUp()
        {
            MockedConfigurationManager = new Mock<IConfigurationManager>();
            MockedHttpRuntime = new Mock<IHttpRuntime>();

            ConfigSettings = new Core.Impl.ConfigSettings(MockedHttpRuntime.Object, MockedConfigurationManager.Object);
        }
    }
}
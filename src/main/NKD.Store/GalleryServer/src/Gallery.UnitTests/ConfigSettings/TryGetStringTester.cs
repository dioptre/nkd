using System;
using Gallery.Core.Exceptions;
using NUnit.Framework;

namespace Gallery.UnitTests.ConfigSettings
{
    public class TryGetStringTester : ConfigSettingsTester
    {
        [Test]
        public void ShouldNotThrowWhenManagerThrowsMissingAppSettingException()
        {
            const string key = "foo";
            MockedConfigurationManager.Setup(cm => cm.GetString(key)).Throws(new MissingAppSettingException(key));

            TestDelegate methodThatShouldNotThrow = () => ConfigSettings.TryGetString(key);

            Assert.DoesNotThrow(methodThatShouldNotThrow, "The MissingAppSettingException should have been caught and handled.");
        }

        [Test]
        public void ShouldReturnEmptyStringWhenMissingAppSettingExceptionThrown()
        {
            const string key = "foo";
            MockedConfigurationManager.Setup(cm => cm.GetString(key)).Throws(new MissingAppSettingException(key));

            string value = ConfigSettings.TryGetString(key);

            Assert.AreEqual(string.Empty, value, "Empty string should have been returned.");
        }

        [Test]
        public void ShouldThrowWhenManagerThrowsException()
        {
            const string key = "foo";
            MockedConfigurationManager.Setup(cm => cm.GetString(key)).Throws(new Exception());

            TestDelegate methodThatShouldThrow = () => ConfigSettings.TryGetString(key);

            Assert.Throws<Exception>(methodThatShouldThrow, "The Exception should have been thrown.");
        }

        [Test]
        public void ShouldReturnWhatManagerReturns()
        {
            const string expectedValue = "expected value";
            const string key = "foo";
            MockedConfigurationManager.Setup(cm => cm.GetString(key)).Returns(expectedValue);

            string value = ConfigSettings.TryGetString(key);

            Assert.AreEqual(expectedValue, value, "Value from ConfigurationManager should have been returned.");
        }
    }
}
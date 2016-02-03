using System;
using System.Net;
using System.ServiceModel.Web;
using System.Text;
using NUnit.Framework;

namespace Gallery.UnitTests.TestHelpers
{
    public static class ExceptionAssert
    {
        private static void CheckTestDelegateCodeForWebFaultException<T>(TestDelegate code)
        {
            code();
            Assert.Fail("WebFaultException<{0}> should have been thrown.", typeof(T).Name);
        }

        public static void ThrowsWebFaultException<T>(TestDelegate code)
        {
            try
            {
                CheckTestDelegateCodeForWebFaultException<T>(code);
            }
            catch (WebFaultException<T>)
            { }
        }

        public static void ThrowsWebFaultException<T>(TestDelegate code, HttpStatusCode expectedStatusCode, string message = "")
        {
            try
            {
                CheckTestDelegateCodeForWebFaultException<T>(code);
            }
            catch (WebFaultException<T> webFaultException)
            {
                var failureMessageBuilder = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(message))
                {
                    failureMessageBuilder.AppendLine(message);
                }
                failureMessageBuilder.AppendFormat("HttpStatusCode {0} expected in exception.", expectedStatusCode);
                Assert.AreEqual(expectedStatusCode, webFaultException.StatusCode, failureMessageBuilder.ToString());
            }
        }

        public static void Throws<TException>(TestDelegate code, TException expectedException, string message, params object[] args)
            where TException : Exception
        {
            TException thrownException = Assert.Throws<TException>(code, message, args);
            Assert.AreEqual(expectedException, thrownException, "The incorrect exception was thrown.");
        }
    }
}
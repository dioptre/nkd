using Moq;
using Moq.Language.Flow;

namespace Gallery.Plugins.UnitTests
{
    public static class MoqExtensions
    {
        /// <summary>
        /// Moq's Setup() does not work for ToString since using ToString invokes it on the Moq object itself.
        /// Use this method to setup a ToString call instead.
        /// </summary>
        /// <typeparam name="TMock"></typeparam>
        /// <param name="mock"></param>
        /// <returns></returns>
        public static ISetup<IToStringable, string> SetupToString<TMock>(this Mock<TMock> mock)
            where TMock : class
        {
            return mock.As<IToStringable>().Setup(m => m.ToString());
        }

        public interface IToStringable
        {
            string ToString();
        }
    }
}
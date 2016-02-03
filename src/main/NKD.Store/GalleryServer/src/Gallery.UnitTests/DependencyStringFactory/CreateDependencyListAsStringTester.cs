using System;
using System.Collections.Generic;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using NUnit.Framework;

namespace Gallery.UnitTests.DependencyStringFactory
{
    [TestFixture]
    public class CreateDependencyListAsStringTester
    {
        private readonly IDependencyStringFactory _dependencyStringFactory = new Core.Impl.DependencyStringFactory();

        [Test]
        public void ShouldReturnEmptyStringWhenGivenNullCollection()
        {
            IEnumerable<Dependency> nullCollection = null;

            string dependencyListAsString = _dependencyStringFactory.CreateDependencyListAsString(nullCollection);

            Assert.IsEmpty(dependencyListAsString, "Empty string should have been returned.");
        }

        [Test]
        public void ShouldSeparateMultipleEntriesWithPipeCharacter()
        {
            const string separator = "|";
            Dependency dependencyOne = CreateDependency();
            Dependency dependencyTwo = CreateDependency();
            string expectedString = _dependencyStringFactory.CreateDependencyString(dependencyOne) + separator +
                _dependencyStringFactory.CreateDependencyString(dependencyTwo);
            IEnumerable<Dependency> dependencies = new[] {dependencyOne, dependencyTwo};

            string dependencyListAsString = _dependencyStringFactory.CreateDependencyListAsString(dependencies);

            StringAssert.AreEqualIgnoringCase(expectedString, dependencyListAsString, "Dependency strings should be separated by '{0}'.", separator);
        }

        private static Dependency CreateDependency()
        {
            return new Dependency { Name = "Package-" + Guid.NewGuid(), VersionSpec = "1.0"};
        }
    }
}
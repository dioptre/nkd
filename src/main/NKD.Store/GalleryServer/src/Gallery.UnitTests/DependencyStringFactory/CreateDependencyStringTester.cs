using System;
using Gallery.Core.Domain;
using Gallery.Core.Interfaces;
using NUnit.Framework;

namespace Gallery.UnitTests.DependencyStringFactory
{
    [TestFixture]
    public class CreateDependencyStringTester
    {
        private readonly IDependencyStringFactory _dependencyStringFactory = new Core.Impl.DependencyStringFactory();

        [Test]
        public void ShouldThrowWhenGivenNullDependency()
        {
            Dependency nullDependency = null;

            TestDelegate methodThatShouldThrow = () => _dependencyStringFactory.CreateDependencyString(nullDependency);

            Assert.Throws<ArgumentNullException>(methodThatShouldThrow, "Null parameter should have made method throw.");
        }

        [Test]
        public void ShouldReturnStringContainingNameOfDependency()
        {
            Dependency dependency = new Dependency {Name = "MyDependency"};

            string dependencyString = _dependencyStringFactory.CreateDependencyString(dependency);

            StringAssert.Contains(dependency.Name, dependencyString, "Name should have been included in the dependency string.");
        }

        [Test]
        public void ShouldReturnStringContainingVersionSpec()
        {
            Dependency dependency = new Dependency {VersionSpec = "(1.0,4.5]"};

            string dependencyString = _dependencyStringFactory.CreateDependencyString(dependency);

            StringAssert.Contains(dependency.VersionSpec, dependencyString, "VersionSpec should have been included in the dependency string.");
        }

        [Test]
        public void ShouldReturnNameFollowedByVersionSpecSeparatedByColon()
        {
            Dependency dependency = new Dependency {Name = "PackageId", VersionSpec = "1.0"};
            string expectedDependencyString = string.Format("{0}:{1}", dependency.Name, dependency.VersionSpec);

            string dependencyString = _dependencyStringFactory.CreateDependencyString(dependency);

            StringAssert.AreEqualIgnoringCase(expectedDependencyString, dependencyString, "Dependency string was formatted incorrectly.");
        }
    }
}
using System;
using NUnit.Framework;

namespace Gallery.UnitTests.RepositoryBase
{
    public class CreateTester : RepositoryBaseTester
    {
        [Test]
        public void ShouldAddItemToDbSet()
        {
            const string stringToAdd = "foo";

            FakeStringRepository.Create(stringToAdd);

            CollectionAssert.Contains(FakeStringRepository.FakeSet.Collection, stringToAdd, "String was not added.");
        }

        [Test]
        public void ShouldRemoveItemFromSetAndRethrowWhenSaveChangesThrows()
        {
            const string stringToAdd = "foobar";
            MockedDatabaseContext.Setup(dc => dc.SaveChanges()).Throws(new Exception());

            TestDelegate methodThatShouldThrow = () => FakeStringRepository.Create(stringToAdd);

            Assert.Throws<Exception>(methodThatShouldThrow, "Create did not rethrow exception.");
            CollectionAssert.DoesNotContain(FakeStringRepository.FakeSet.Collection, stringToAdd, "String should not have been added.");
            Assert.IsTrue(FakeStringRepository.FakeSet.RemoveWasCalled, "Object was not removed from the set on exception.");
        }
    }
}
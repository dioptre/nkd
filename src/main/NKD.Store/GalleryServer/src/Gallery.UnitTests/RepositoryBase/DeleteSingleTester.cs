using System.Collections.Generic;
using NUnit.Framework;

namespace Gallery.UnitTests.RepositoryBase
{
    public class DeleteSingleTester : RepositoryBaseTester
    {
        [Test]
        public void ShouldRemoveStringFromDbSetWhenItIsTheOnlyInstanceInDbSet()
        {
            const string stringToDelete = "foot";
            FakeStringRepository.FakeSet.Collection = new List<string> {stringToDelete, "hand"};

            FakeStringRepository.DeleteSingle(s => s == stringToDelete);

            Assert.IsTrue(FakeStringRepository.FakeSet.RemoveWasCalled, "Remove should have been called.");
        }

        [Test]
        public void ShouldNotRemoveStringFromDbSetWhenItDoesNotExist()
        {
            const string stringToDelete = "foot";
            FakeStringRepository.FakeSet.Collection = new List<string> { "hand", "shoulder", "arm" };

            FakeStringRepository.DeleteSingle(s => s == stringToDelete);

            Assert.IsFalse(FakeStringRepository.FakeSet.RemoveWasCalled, "Remove should not have been called.");
        }

        [Test]
        public void ShouldNotRemoveStringFromDbSetWhenMultipleInstancesExist()
        {
            const string stringToDelete = "foot";
            FakeStringRepository.FakeSet.Collection = new List<string> { stringToDelete, stringToDelete, "shoulder" };

            FakeStringRepository.DeleteSingle(s => s == stringToDelete);

            Assert.IsFalse(FakeStringRepository.FakeSet.RemoveWasCalled, "Remove should not have been called.");
        }
    }
}
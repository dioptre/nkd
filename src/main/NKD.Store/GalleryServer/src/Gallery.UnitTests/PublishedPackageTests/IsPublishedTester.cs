//using System;
//using Gallery.Infrastructure.FeedModels;
//using NUnit.Framework;

//namespace Gallery.UnitTests.PublishedPackageTests
//{
//    [TestFixture]
//    public class IsPublishedTester
//    {
//        [Test]
//        public void ShouldBeTrueWhenPublishedIsNotNull()
//        {
//            var publishedPackage = new PublishedPackage {Published = DateTime.Now};

//            bool isPublished = publishedPackage.IsPublished;

//            Assert.IsTrue(isPublished, "Published Package should be considered Published when its Published Date has value.");
//        }

//        [Test]
//        public void ShouldBeFalseWhenPublishedIsNull()
//        {
//            var publishedPackage = new PublishedPackage { Published = null };

//            bool isPublished = publishedPackage.IsPublished;

//            Assert.IsFalse(isPublished, "Published Package should not be considered Published when its Published Date has no value.");
//        }
//    }
//}
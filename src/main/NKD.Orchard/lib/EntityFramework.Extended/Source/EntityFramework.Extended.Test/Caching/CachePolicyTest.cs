﻿using System.Runtime.Caching;
using EntityFramework.Caching;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EntityFramework.Test.Caching
{


    [TestClass]
    public class CachePolicyTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CachePolicyConstructorTest()
        {
            var cachePolicy = new CachePolicy();
            
            cachePolicy.Should().NotBeNull();
            cachePolicy.Mode.Should().Be(CacheExpirationMode.None);
            cachePolicy.AbsoluteExpiration.Should().Be(ObjectCache.InfiniteAbsoluteExpiration);
            cachePolicy.SlidingExpiration.Should().Be(ObjectCache.NoSlidingExpiration);
        }

        [TestMethod]
        public void WithAbsoluteExpirationTest()
        {
            var absoluteExpiration = new DateTimeOffset(2012, 1, 1, 12, 0, 0, TimeSpan.Zero);
            var cachePolicy = CachePolicy.WithAbsoluteExpiration(absoluteExpiration);
            
            cachePolicy.Should().NotBeNull();
            cachePolicy.Mode.Should().Be(CacheExpirationMode.Absolute);
            cachePolicy.AbsoluteExpiration.Should().Be(absoluteExpiration);
            cachePolicy.SlidingExpiration.Should().Be(ObjectCache.NoSlidingExpiration);
        }

        [TestMethod]
        public void WithSlidingExpirationTest()
        {
            TimeSpan slidingExpiration = TimeSpan.FromMinutes(5);
            
            var cachePolicy = CachePolicy.WithSlidingExpiration(slidingExpiration);
            cachePolicy.Should().NotBeNull();
            cachePolicy.Mode.Should().Be(CacheExpirationMode.Sliding);
            cachePolicy.AbsoluteExpiration.Should().Be(ObjectCache.InfiniteAbsoluteExpiration);
            cachePolicy.SlidingExpiration.Should().Be(slidingExpiration);
        }

        [TestMethod]
        public void WithDurationExpirationTest()
        {
            TimeSpan expirationSpan = TimeSpan.FromSeconds(30);
            var cachePolicy = CachePolicy.WithDurationExpiration(expirationSpan);

            cachePolicy.Should().NotBeNull();
            cachePolicy.Mode.Should().Be(CacheExpirationMode.Duration);
            cachePolicy.AbsoluteExpiration.Should().Be(ObjectCache.InfiniteAbsoluteExpiration);
            cachePolicy.SlidingExpiration.Should().Be(ObjectCache.NoSlidingExpiration);
            cachePolicy.Duration.Should().Be(expirationSpan);
        }

    }
}

using System;
using System.Threading;
using Gallery.Core.Interfaces;
using NUnit.Framework;

namespace Gallery.UnitTests.TaskScheduler
{
    [TestFixture]
    public class ScheduleTaskTester
    {
        private const int NUM_THREADS = 200;
        private const int LOOP_COUNT = 10;

        [Test]
        public void QueueUserWorkItemShouldProcessSimultaneousRequestsSequentially()
        {
            // Arrange
            const int expected = NUM_THREADS * LOOP_COUNT;
            var countdownEvent = new CountdownEvent(expected);
            ITaskScheduler singleThreadedTaskScheduler = new Core.Impl.SequentialTaskScheduler(null);
            Helper volatileFieldHelper = new Helper();

            // Act
            for (int i = 0; i < NUM_THREADS; i++)
            {
                new Thread(() =>
                {
                    for (int j = 0; j < LOOP_COUNT; j++)
                    {
                        singleThreadedTaskScheduler.ScheduleTask(() =>
                        {
                            int initialCount = volatileFieldHelper.Field;
                            Thread.Yield();
                            volatileFieldHelper.Field = initialCount + 1;
                            countdownEvent.Signal();
                        });
                    }
                }).Start();
            }

            // Assert
            Assert.IsTrue(countdownEvent.Wait(TimeSpan.FromSeconds(5)), "CountdownEvent timed out waiting to be signaled.");
            Assert.AreEqual(expected, volatileFieldHelper.Field, "Wrong count - race condition?");
        }

        public class Helper
        {
            public volatile int Field;
        }

    }
}
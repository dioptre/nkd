using System;
using Gallery.Core.Interfaces;

namespace Gallery.UnitTests.TestHelpers
{
    public class FakeTaskScheduler : ITaskScheduler
    {
        public void ScheduleTask(Action actionToInvoke)
        {
            actionToInvoke.Invoke();
        }
    }
}
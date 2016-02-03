using System;

namespace Gallery.Core.Interfaces
{
    public interface ITaskScheduler
    {
        void ScheduleTask(Action actionToInvoke);
    }
}
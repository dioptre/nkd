using System;
using System.Threading.Tasks;
using Gallery.Core.Interfaces;
using Ninject.Extensions.Logging;

namespace Gallery.Core.Impl
{
    public class SequentialTaskScheduler : ITaskScheduler
    {
        private static readonly object LockObj = new object();
        private static readonly TaskFactory TaskFactory = new TaskFactory(TaskScheduler.Default);
        private static Task _lastTask;

        private readonly ILogger _logger;

        public SequentialTaskScheduler(ILogger logger)
        {
            _logger = logger;
        }

        public void ScheduleTask(Action actionToInvoke)
        {
            Action safeWrapAction = () =>
            {
                try { actionToInvoke(); }
                catch(Exception ex)
                {
                    _logger.Error(ex, "Exception occurred when trying to execute an Action in SingleThreadedThreadPool.");
                }
            };

            lock (LockObj)
            {
                _lastTask = (_lastTask != null)
                    ? _lastTask.ContinueWith(_ => safeWrapAction(), TaskScheduler.Default)
                    : TaskFactory.StartNew(safeWrapAction);
            }
        }
    }
}
using System.Threading;
using Gallery.Core;
using Gallery.Core.Interfaces;
using Gallery.DependencyResolution;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gallery.LoadTests
{
    [TestClass]
    public class TaskSchedulerTester
    {
        private readonly ITaskScheduler _taskScheduler;

        public TaskSchedulerTester()
        {
            DependencyRegistrar.EnsureDependenciesRegistered();
            _taskScheduler = IoC.Resolver.Resolve<ITaskScheduler>();
        }

        [TestMethod]
        public void ShouldQueueMethodWithoutBlowingUp()
        {
            _taskScheduler.ScheduleTask(() => Thread.Sleep(1000));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Activities;
using System.Activities.Hosting;
using System.Threading;

namespace NKD.Workflow.Helpers
{

    class WorkflowInstanceExtensionHelper : IWorkflowInstanceExtension
    {
        private WorkflowInstanceProxy _instance;
        public IEnumerable<object> GetAdditionalExtensions()
        {
            return null;
        }

        public void SetInstance(WorkflowInstanceProxy instance)
        {
            _instance = instance;
        }


        public void WaitSome(Bookmark bookmark)
        {
            ThreadPool.QueueUserWorkItem(state =>
                {
                    Thread.Sleep(1000);
                    var ias = _instance.BeginResumeBookmark(bookmark, 42, null, null);

                    var result = _instance.EndResumeBookmark(ias);

                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debug.WriteLine("BookmarkResumptionResult: '{0}'", result);
                    }

                });
        }

    }
}

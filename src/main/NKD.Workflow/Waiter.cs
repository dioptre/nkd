using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Activities;
using System.Activities.Hosting;


namespace NKD.Workflow
{

    //Original code: http://archive.msdn.microsoft.com/wf4Callbacks
    public class Waiter
    {

        public class WaiterEventArgs : EventArgs
        {
            public object Message { get; set; }
        }

        public delegate void WaiterEventHandler(object sender, WaiterEventArgs e);
        public event WaiterEventHandler Continue;

        public void ExecuteWaiter(object message)
        {
            if (Continue != null)
            {
                Continue(this, new WaiterEventArgs() { Message = message });
            }
        }

        public sealed class WaiterActivity : NativeActivity<object>
        {
            private readonly Variable<NoPersistHandle> _noPersistHandle = new Variable<NoPersistHandle>();
            private BookmarkCallback _waiterBookmarkCallback;

            [RequiredArgument]
            public InArgument<string> BookmarkName { get; set; }
            public InArgument<Waiter> Waiter { get; set; }

            public BookmarkCallback WaiterBookmarkCallback
            {
                get
                {
                    return _waiterBookmarkCallback ??
                            (_waiterBookmarkCallback = new BookmarkCallback(OnWaiterCallback));
                }
            }

            protected override bool CanInduceIdle
            {
                get
                {
                    return true;
                }
            }

            protected override void CacheMetadata(NativeActivityMetadata metadata)
            {
                // Tell the runtime that we need this extension
                metadata.RequireExtension(typeof(WaiterExtension));

                // Provide a Func to create the extension if it does not already exist
                metadata.AddDefaultExtensionProvider(() => new WaiterExtension());

                metadata.AddArgument(new RuntimeArgument("Waiter", typeof(Waiter), ArgumentDirection.In, true));
                metadata.AddArgument(new RuntimeArgument("Result", typeof(object), ArgumentDirection.Out, false));
                metadata.AddImplementationVariable(_noPersistHandle);
            }

            protected override void Execute(NativeActivityContext context)
            {
                // Enter a no persist zone to pin this activity to memory since we are setting up a delegate to receive a callback
                var handle = _noPersistHandle.Get(context);
                handle.Enter(context);

                // Get (which may create) the extension
                var waiterExtension = context.GetExtension<WaiterExtension>();

                // Add the callback
                waiterExtension.AddWaiterCallback(Waiter.Get(context), new Bookmark(BookmarkName.Get(context)));

                // Set a bookmark - the extension will resume when the Waiter is fired
                context.CreateBookmark(BookmarkName.Get(context), WaiterBookmarkCallback);
            }

            internal void OnWaiterCallback(NativeActivityContext context, Bookmark bookmark, Object value)
            {
                // Store the result
                Result.Set(context, value);

                // Exit the no persist zone 
                var handle = _noPersistHandle.Get(context);
                handle.Exit(context);
            }
        }

        internal class WaiterExtension : IWorkflowInstanceExtension
        {
            private static bool _addedCallback;
            private WorkflowInstanceProxy _instance;
            private Bookmark _bookmark { get; set; }

            #region IWorkflowInstanceExtension Members

            public IEnumerable<object> GetAdditionalExtensions()
            {
                return null;
            }

            public void SetInstance(WorkflowInstanceProxy instance)
            {
                _instance = instance;
            }

            #endregion

            internal void AddWaiterCallback(Waiter waiter, Bookmark bookmark)
            {
                if (!_addedCallback)
                {
                    _bookmark = bookmark;
                    _addedCallback = true;
                    waiter.Continue += OnWaiterFired;
                }
            }



            internal void OnWaiterFired(object sender, WaiterEventArgs args)
            {
                // Waiter was fired, resume the bookmark
                _instance.BeginResumeBookmark(
                    _bookmark,
                    args.Message,
                    (asr) => _instance.EndResumeBookmark(asr),
                    null);
            }
        }
    }
}

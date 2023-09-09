using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	/// <summary>
	/// Synchronization context suitable for testing animation tasks
	/// </summary>
	/// <remarks>
	/// Animations in an app run on the UI thread; all of the async operations synchronize back to the UI thread's context.
	/// But async operations in unit tests don't have a single-threaded sychronization context by default, so they fall back
	/// to the default behavior of scheduling their continuations on the thread pool. To accurately test animation operations
	/// asynchronously (they way they behave when animating properties in an app), we need them to use a single-threaded
	/// context. So we provide this one for testing. It queues operations up and executes them in order on the current thread,
	/// just like the UI thread does.
	/// </remarks>
	sealed class SingleThreadSynchronizationContext : SynchronizationContext
	{
		readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> _queue = new();

		public override void Post(SendOrPostCallback d, object state)
		{
			ArgumentNullException.ThrowIfNull(d);
			_queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
		}

		void RunOnCurrentThread()
		{
			foreach (var workItem in _queue.GetConsumingEnumerable())
			{
				workItem.Key(workItem.Value);
			}
		}

		void Complete()
		{
			_queue.CompleteAdding();
		}

		public static T Run<T>(Func<Task<T>> asyncMethod)
		{
			ArgumentNullException.ThrowIfNull(asyncMethod);

			var previousContext = Current;
			try
			{
				var context = new SingleThreadSynchronizationContext();
				SetSynchronizationContext(context);

				// Invoke the function and alert the context when it's complete
				var task = asyncMethod() ?? throw new InvalidOperationException("No task provided.");
				task.ContinueWith(delegate { context.Complete(); }, TaskScheduler.Default);

				// Start working through the queue				
				context.RunOnCurrentThread();
				return task.GetAwaiter().GetResult();
			}
			finally
			{
				SetSynchronizationContext(previousContext);
			}
		}

	}
}
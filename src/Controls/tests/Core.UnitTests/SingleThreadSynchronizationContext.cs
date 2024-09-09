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
	/// But async operations in unit tests don't have a single-threaded synchronization context by default, so they fall back
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

			if (!_queue.IsAddingCompleted)
			{
				_queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
			}
		}

		public void RunOnCurrentThread()
		{
			foreach (var workItem in _queue.GetConsumingEnumerable())
			{
				workItem.Key(workItem.Value);
			}
		}

		public async Task Complete()
		{
			_queue.CompleteAdding();
		}

	}

	internal static class SingleThreadSimulator
	{
		public static async Task Run(Func<Task> asyncMethod)
		{
			ArgumentNullException.ThrowIfNull(asyncMethod);

			// Save off the old context so we can restore it when we're done
			var previousContext = SynchronizationContext.Current;

			try
			{
				var context = new SingleThreadSynchronizationContext();
				SynchronizationContext.SetSynchronizationContext(context);

				// Invoke the function and alert the context when it's complete
				var task = asyncMethod() ?? throw new InvalidOperationException("No task provided.");

				_ = task.ContinueWith(async (t, o) => await context.Complete(), TaskScheduler.Default);

				// Start working through the queue
				context.RunOnCurrentThread();

				await task;
			}
			finally
			{
				SynchronizationContext.SetSynchronizationContext(previousContext);
			}
		}
	}
}
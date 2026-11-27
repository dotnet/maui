using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core)]
	public class OneTimeInitializationActionTests
	{
		[Fact]
		public void ConcurrentCallerWaitsForInitializationToComplete()
		{
			using var initializationStarted = new ManualResetEventSlim();
			using var releaseInitialization = new ManualResetEventSlim();
			using var secondCallerStarted = new ManualResetEventSlim();
			var initializationCompleted = false;
			bool? secondCallerObservedCompletion = null;
			var initialization = new OneTimeInitializationAction(() =>
			{
				initializationStarted.Set();
				releaseInitialization.Wait();
				Volatile.Write(ref initializationCompleted, true);
			});

			// Dedicated threads guarantee both callers are running regardless of thread pool availability.
			var firstCaller = new Thread(initialization.InvokeOnce) { IsBackground = true };
			var secondCaller = new Thread(() =>
			{
				secondCallerStarted.Set();
				initialization.InvokeOnce();
				secondCallerObservedCompletion = Volatile.Read(ref initializationCompleted);
			})
			{ IsBackground = true };

			try
			{
				firstCaller.Start();
				Assert.True(initializationStarted.Wait(TimeSpan.FromSeconds(10)));

				secondCaller.Start();
				Assert.True(secondCallerStarted.Wait(TimeSpan.FromSeconds(10)));

				// The second caller must block while initialization is still running.
				Assert.False(secondCaller.Join(TimeSpan.FromMilliseconds(200)));
			}
			finally
			{
				releaseInitialization.Set();
			}

			Assert.True(firstCaller.Join(TimeSpan.FromSeconds(10)));
			Assert.True(secondCaller.Join(TimeSpan.FromSeconds(10)));
			Assert.True(secondCallerObservedCompletion);
		}

		[Fact]
		public void ConcurrentCallersInitializeOnce()
		{
			var invocationCount = 0;
			var initialization = new OneTimeInitializationAction(
				() => Interlocked.Increment(ref invocationCount));

			Parallel.For(0, 32, _ => initialization.InvokeOnce());

			Assert.Equal(1, invocationCount);
		}
	}
}

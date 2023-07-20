using System;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Dispatcher)]
	public class DispatcherTests : TestBase
	{
		// just a check to make sure the test dispatcher is working
		[Fact]
		public Task TestImplementationHasDispatcher() =>
			InvokeOnMainThreadAsync(() =>
			{
				var dispatcher = Dispatcher.GetForCurrentThread();

				Assert.NotNull(dispatcher);
				Assert.False(dispatcher.IsDispatchRequired);
			});

		[Fact]
		public Task BackgroundThreadDoesNotHaveDispatcher() =>
			Task.Run(() =>
			{
				var dispatcher = Dispatcher.GetForCurrentThread();

				Assert.Null(dispatcher);
			});

		[Fact]
		public Task BackgroundThreadDoesNotGetDispatcherFromMainThread() =>
			InvokeOnMainThreadAsync(async () =>
			{
				var dispatcher = Dispatcher.GetForCurrentThread();
				Assert.NotNull(dispatcher);

				await Task.Run(() =>
				{
					var dispatcher = Dispatcher.GetForCurrentThread();
					Assert.Null(dispatcher);
				});
			});

		[Fact]
		public Task SameDispatcherForSameThread() =>
			InvokeOnMainThreadAsync(() =>
			{
				var dispatcher1 = Dispatcher.GetForCurrentThread();
				var dispatcher2 = Dispatcher.GetForCurrentThread();

				Assert.Same(dispatcher1, dispatcher2);
			});

		[Fact]
		public Task DifferentDispatcherForDifferentThread() =>
			InvokeOnMainThreadAsync(async () =>
			{
				var outerId = Environment.CurrentManagedThreadId;
				var dispatcher1 = Dispatcher.GetForCurrentThread();

				await Task.Run(() =>
				{
					var innerId = Environment.CurrentManagedThreadId;
					var dispatcher2 = Dispatcher.GetForCurrentThread();

					Assert.NotEqual(outerId, innerId);
					Assert.NotSame(dispatcher1, dispatcher2);
				});
			});

		[Fact]
		public Task DispatchDelayedPostsOnCorrectDispatcher() =>
			InvokeOnMainThreadAsync(async () =>
			{
				var dispatcher = Dispatcher.GetForCurrentThread();

				var tcs = new TaskCompletionSource<IDispatcher>();

				var result = dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
				{
					var innerDispatcher = Dispatcher.GetForCurrentThread();
					tcs.SetResult(innerDispatcher);
				});

				Assert.True(result);

				var innerDispatcher = await tcs.Task;

				Assert.Same(dispatcher, innerDispatcher);
			});

		[Fact]
		public Task DispatchDelayedIsNotImmediate() =>
			InvokeOnMainThreadAsync(async () =>
			{
				var dispatcher = Dispatcher.GetForCurrentThread();

				var tcs = new TaskCompletionSource<DateTime>();

				var now = DateTime.Now;

				var result = dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
				{
					var later = DateTime.Now;
					tcs.SetResult(later);
				});

				Assert.True(result);

				var later = await tcs.Task;
				var duration = (later - now).TotalMilliseconds;

				Assert.True(duration > 450);
			});

		[Fact]
		public Task CreateTimerIsNotNull() =>
			InvokeOnMainThreadAsync(() =>
			{
				var dispatcher = Dispatcher.GetForCurrentThread();

				var timer = dispatcher.CreateTimer();

				Assert.NotNull(timer);
			});

		[Fact]
		public Task CreateTimerIsInExpectedState() =>
			InvokeOnMainThreadAsync(() =>
			{
				var dispatcher = Dispatcher.GetForCurrentThread();

				var timer = dispatcher.CreateTimer();

				Assert.False(timer.IsRunning);
				Assert.True(timer.IsRepeating);
			});

		[Fact]
		public Task CreateTimerNonRepeatingDoesNotRepeat() =>
			InvokeOnMainThreadAsync(async () =>
			{
				var dispatcher = Dispatcher.GetForCurrentThread();

				var ticks = 0;

				var timer = dispatcher.CreateTimer();
				using var disposer = new TimerDisposer(timer);

				Assert.False(timer.IsRunning);

				timer.Interval = TimeSpan.FromMilliseconds(200);
				timer.IsRepeating = false;

				timer.Tick += (_, _) =>
				{
					ticks++;
				};

				timer.Start();

				Assert.True(timer.IsRunning);

				await Task.Delay(TimeSpan.FromSeconds(1.1));

				Assert.Equal(1, ticks);
			});

		[Fact]
		public Task CreateTimerRepeatingRepeats() =>
			InvokeOnMainThreadAsync(async () =>
			{
				var dispatcher = Dispatcher.GetForCurrentThread();

				var ticks = 0;

				var timer = dispatcher.CreateTimer();
				using var disposer = new TimerDisposer(timer);

				Assert.False(timer.IsRunning, "Timer was running BEFORE it was started.");

				timer.Interval = TimeSpan.FromMilliseconds(200);
				timer.IsRepeating = true;

				TaskCompletionSource taskCompletionSource = new TaskCompletionSource();

				timer.Tick += (_, _) =>
				{
					Assert.True(timer.IsRunning, "Timer was not running DURING the tick.");
					ticks++;

					if (ticks > 1)
						taskCompletionSource.SetResult();
				};

				timer.Start();

				Assert.True(timer.IsRunning, "Timer was not running AFTER the tick.");

				try
				{
					await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
				}
				catch (TaskCanceledException)
				{
					// If the task is cancelled we want it to just fall through to the assert
				}

				// If it's repeating, ticks will be greater than 1
				Assert.True(ticks > 1, $"# of Ticks: {ticks}, expected > 1");
			});

		[Fact]
		public async Task NonRepeatingTimerIsStoppedAfterFiring()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var dispatcher = Dispatcher.GetForCurrentThread();
				var timer = dispatcher.CreateTimer();
				using var disposer = new TimerDisposer(timer);

				Assert.False(timer.IsRunning);

				timer.Interval = TimeSpan.FromMilliseconds(200);
				timer.IsRepeating = false;

				timer.Tick += (_, _) => { };
				timer.Start();

				await Task.Delay(TimeSpan.FromSeconds(1));
				Assert.False(timer.IsRunning, "The timer is not repeating, so it should no longer be marked as running after the first tick.");
			});
		}

		class TimerDisposer : IDisposable
		{
			IDispatcherTimer _timer;

			public TimerDisposer(IDispatcherTimer timer)
			{
				_timer = timer;
			}

			public void Dispose()
			{
				_timer.Stop();
			}
		}
	}
}
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using Xunit;

namespace Microsoft.Maui.UnitTests.Dispatching
{
	// All these tests should run in a separate thread/task to avoid polluting the other tests.
	// This is only because the dispatcher and dispatcher provider are both "static" classes.
	// Technically these tests are useless because they cannot test shipping code as they are
	// none of the platforms. However, they sort of do test the test dispatcher...
	[Category(TestCategory.Core, TestCategory.Dispatching)]
	public class DispatcherTests : IDisposable
	{
		DispatcherProviderStub _dispatcherProvider;

		public DispatcherTests()
		{
			_dispatcherProvider = new DispatcherProviderStub();
			DispatcherProvider.SetCurrent(_dispatcherProvider);
		}

		public void Dispose()
		{
			DispatcherProvider.SetCurrent(null);
			_dispatcherProvider.Dispose();
		}

		// just a check to make sure the test dispatcher is working
		[Fact]
		public Task TestImplementationHasDispatcher() =>
			DispatcherTest.Run(() =>
			{
				Assert.False(DispatcherProviderStubOptions.SkipDispatcherCreation);

				var dispatcher = Dispatcher.GetForCurrentThread();

				Assert.NotNull(dispatcher);
				Assert.False(dispatcher.IsDispatchRequired);
			});

		[Fact]
		public Task BackgroundThreadDoesNotHaveDispatcher() =>
			DispatcherTest.Run(() =>
			{
				// act like the real world
				DispatcherProviderStubOptions.SkipDispatcherCreation = true;

				var dispatcher = Dispatcher.GetForCurrentThread();

				Assert.Null(dispatcher);
			});

		[Fact]
		public Task BackgroundThreadDoesNotGetDispatcherFromMainThread() =>
			DispatcherTest.Run(async () =>
			{
				var dispatcher = Dispatcher.GetForCurrentThread();
				Assert.NotNull(dispatcher);

				await Task.Run(() =>
				{
					DispatcherProviderStubOptions.SkipDispatcherCreation = true;

					var dispatcher = Dispatcher.GetForCurrentThread();
					Assert.Null(dispatcher);
				});
			});

		[Fact]
		public Task SameDispatcherForSameThread() =>
			DispatcherTest.Run(() =>
			{
				var dispatcher1 = Dispatcher.GetForCurrentThread();
				var dispatcher2 = Dispatcher.GetForCurrentThread();

				Assert.Same(dispatcher1, dispatcher2);
			});

		[Fact]
		public Task DifferentDispatcherForDifferentThread() =>
			DispatcherTest.Run(async () =>
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
		public Task DispatchDelayedIsNotImmediate() =>
			DispatcherTest.Run(async () =>
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
			DispatcherTest.Run(() =>
			{
				var dispatcher = Dispatcher.GetForCurrentThread();

				var timer = dispatcher.CreateTimer();

				Assert.NotNull(timer);
			});

		[Fact]
		public Task CreateTimerNonRepeatingDoesNotRepeat() =>
			DispatcherTest.Run(async () =>
			{
				var dispatcher = Dispatcher.GetForCurrentThread();

				var ticks = 0;

				var timer = dispatcher.CreateTimer();

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
			DispatcherTest.Run(async () =>
			{
				var dispatcher = Dispatcher.GetForCurrentThread();

				var ticks = 0;

				var timer = dispatcher.CreateTimer();

				Assert.False(timer.IsRunning);

				timer.Interval = TimeSpan.FromMilliseconds(200);
				timer.IsRepeating = true;

				timer.Tick += (_, _) =>
				{
					ticks++;
				};

				timer.Start();

				Assert.True(timer.IsRunning);

				// Give it time to repeat at least once
				await Task.Delay(TimeSpan.FromSeconds(1));

				// If it's repeating, ticks will be greater than 1
				Assert.True(ticks > 1);
			});

		[Fact]
		public Task DispatchIfRequired_ShouldCallDispatch_WhenDispatchIsRequired() =>
			DispatcherTest.Run(() =>
			{
				// Arrange
				var dispatcher = new TestDispatcher();
				dispatcher.SetIsDispatchRequired(true);

				int executionCount = 0;
				Action testAction = () => executionCount++;

				// Act
				dispatcher.DispatchIfRequired(testAction);

				// Assert
				Assert.Equal(1, executionCount);
				Assert.True(dispatcher.WasDispatchCalled);
			});

		[Fact]
		public Task DispatchIfRequired_ShouldExecuteAction_WhenDispatchIsNotRequired() =>
			DispatcherTest.Run(() =>
			{
				// Arrange
				var dispatcher = new TestDispatcher();
				dispatcher.SetIsDispatchRequired(false);

				int executionCount = 0;
				Action testAction = () => executionCount++;

				// Act
				dispatcher.DispatchIfRequired(testAction);

				// Assert
				Assert.Equal(1, executionCount);
				Assert.False(dispatcher.WasDispatchCalled);
			});

		[Fact]
		public Task DispatchIfRequiredAsync_ShouldCallDispatchAsync_WhenDispatchIsRequired() =>
			DispatcherTest.Run(async () =>
			{
				// Arrange
				var dispatcher = new TestDispatcher();
				dispatcher.SetIsDispatchRequired(true);

				int executionCount = 0;
				Action testAction = () => executionCount++;

				// Act
				await dispatcher.DispatchIfRequiredAsync(testAction);

				// Assert
				Assert.Equal(1, executionCount);
				Assert.True(dispatcher.WasDispatchCalled);
			});

		[Fact]
		public Task DispatchIfRequiredAsync_ShouldExecuteAction_WhenDispatchIsNotRequired() =>
			DispatcherTest.Run(async () =>
			{
				// Arrange
				var dispatcher = new TestDispatcher();
				dispatcher.SetIsDispatchRequired(false);

				int executionCount = 0;
				Action testAction = () => executionCount++;

				// Act
				await dispatcher.DispatchIfRequiredAsync(testAction);

				// Assert
				Assert.Equal(1, executionCount);
				Assert.False(dispatcher.WasDispatchCalled);
			});

		[Fact]
		public Task DispatchIfRequiredAsync_WithFuncTask_ShouldCallDispatchAsync_WhenDispatchIsRequired() =>
			DispatcherTest.Run(async () =>
			{
				// Arrange
				var dispatcher = new TestDispatcher();
				dispatcher.SetIsDispatchRequired(true);

				int executionCount = 0;
				Func<Task> testFunc = async () => 
				{
					await Task.Delay(1);
					executionCount++;
				};

				// Act
				await dispatcher.DispatchIfRequiredAsync(testFunc);

				// Assert
				Assert.Equal(1, executionCount);
				Assert.True(dispatcher.WasDispatchCalled);
			});

		[Fact]
		public Task DispatchIfRequiredAsync_WithFuncTask_ShouldExecuteFunc_WhenDispatchIsNotRequired() =>
			DispatcherTest.Run(async () =>
			{
				// Arrange
				var dispatcher = new TestDispatcher();
				dispatcher.SetIsDispatchRequired(false);

				int executionCount = 0;
				Func<Task> testFunc = async () => 
				{
					await Task.Delay(1);
					executionCount++;
				};

				// Act
				await dispatcher.DispatchIfRequiredAsync(testFunc);

				// Assert
				Assert.Equal(1, executionCount);
				Assert.False(dispatcher.WasDispatchCalled);
			});

		[Fact]
		public Task DispatchIfRequiredAsync_WithFuncT_ShouldCallDispatchAsync_WhenDispatchIsRequired() =>
			DispatcherTest.Run(async () =>
			{
				// Arrange
				var dispatcher = new TestDispatcher();
				dispatcher.SetIsDispatchRequired(true);

				Func<int> testFunc = () => 42;

				// Act
				var result = await dispatcher.DispatchIfRequiredAsync(testFunc);

				// Assert
				Assert.Equal(42, result);
				Assert.True(dispatcher.WasDispatchCalled);
			});

		[Fact]
		public Task DispatchIfRequiredAsync_WithFuncT_ShouldExecuteFunc_WhenDispatchIsNotRequired() =>
			DispatcherTest.Run(async () =>
			{
				// Arrange
				var dispatcher = new TestDispatcher();
				dispatcher.SetIsDispatchRequired(false);

				Func<int> testFunc = () => 42;

				// Act
				var result = await dispatcher.DispatchIfRequiredAsync(testFunc);

				// Assert
				Assert.Equal(42, result);
				Assert.False(dispatcher.WasDispatchCalled);
			});

		[Fact]
		public Task DispatchIfRequiredAsync_WithFuncTaskT_ShouldCallDispatchAsync_WhenDispatchIsRequired() =>
			DispatcherTest.Run(async () =>
			{
				// Arrange
				var dispatcher = new TestDispatcher();
				dispatcher.SetIsDispatchRequired(true);

				Func<Task<int>> testFunc = async () => 
				{
					await Task.Delay(1);
					return 42;
				};

				// Act
				var result = await dispatcher.DispatchIfRequiredAsync(testFunc);

				// Assert
				Assert.Equal(42, result);
				Assert.True(dispatcher.WasDispatchCalled);
			});

		[Fact]
		public Task DispatchIfRequiredAsync_WithFuncTaskT_ShouldExecuteFunc_WhenDispatchIsNotRequired() =>
			DispatcherTest.Run(async () =>
			{
				// Arrange
				var dispatcher = new TestDispatcher();
				dispatcher.SetIsDispatchRequired(false);

				Func<Task<int>> testFunc = async () => 
				{
					await Task.Delay(1);
					return 42;
				};

				// Act
				var result = await dispatcher.DispatchIfRequiredAsync(testFunc);

				// Assert
				Assert.Equal(42, result);
				Assert.False(dispatcher.WasDispatchCalled);
			});
	}

	// Helper class for testing
	public class TestDispatcher : IDispatcher
	{
		private bool _isDispatchRequired = false;
		public bool WasDispatchCalled { get; private set; }

		public bool IsDispatchRequired => _isDispatchRequired;

		public void SetIsDispatchRequired(bool value)
		{
			_isDispatchRequired = value;
		}

		public bool Dispatch(Action action)
		{
			WasDispatchCalled = true;
			action();
			return true;
		}

		public bool DispatchDelayed(TimeSpan delay, Action action)
		{
			WasDispatchCalled = true;
			Task.Delay(delay).ContinueWith(_ => action());
			return true;
		}

		public IDispatcherTimer CreateTimer()
		{
			return new TestDispatcherTimer();
		}
	}

	// Helper class for testing
	public class TestDispatcherTimer : IDispatcherTimer
	{
		public TimeSpan Interval { get; set; }
		public bool IsRepeating { get; set; }
		public bool IsRunning { get; private set; }

		public event EventHandler? Tick;

		public void Start()
		{
			IsRunning = true;
			// Simplified implementation for testing
			Task.Delay(Interval).ContinueWith(_ => 
			{
				Tick?.Invoke(this, EventArgs.Empty);
				if (!IsRepeating)
				{
					IsRunning = false;
				}
			});
		}

		public void Stop()
		{
			IsRunning = false;
		}
	}
}
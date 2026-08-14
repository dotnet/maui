using System;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Animations;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Animations)]
	public class AnimationManagerTests
	{
		[Fact]
		public void DisposeReleasesOwnedTickerCallbackWithoutDisposingAnimation()
		{
			var ticker = new TestTicker();
			var manager = new AnimationManager(ticker);
			bool stepCalled = false;
			bool finishedCalled = false;
			var animation = new Animation(_ => stepCalled = true, duration: 10, finished: () => finishedCalled = true);

			manager.Add(animation);
			manager.Dispose();

			Assert.Null(ticker.Fire);
			Assert.False(ticker.IsRunning);
			Assert.Equal(1, ticker.StopCount);
			Assert.Equal(1, ticker.DisposeCount);
			Assert.False(animation.IsDisposed);
			Assert.NotNull(animation.Step);
			Assert.NotNull(animation.Finished);
			Assert.False(stepCalled);
			Assert.False(finishedCalled);
		}

		[Fact]
		public void DisposeReleasesAnimationPayloadWhenTickerOutlivesManager()
		{
			var ticker = new TestTicker();
			WeakReference<object> payload = CreateDisposedManagerPayload(ticker);

			CollectGarbage();

			Assert.False(payload.TryGetTarget(out _));
			GC.KeepAlive(ticker);
		}

		[Fact]
		public void DisposeRemovesOwnedCallbackFromMulticastDelegate()
		{
			var ticker = new TestTicker();
			var manager = new AnimationManager(ticker);
			bool stepCalled = false;
			int otherCallbackCount = 0;
			manager.Add(new Animation(_ => stepCalled = true, duration: 10));
			ticker.Fire += () => otherCallbackCount++;

			manager.Dispose();
			ticker.Fire?.Invoke();

			Assert.False(stepCalled);
			Assert.Equal(1, otherCallbackCount);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference<object> CreateDisposedManagerPayload(TestTicker ticker)
		{
			var payload = new object();
			var manager = new AnimationManager(ticker);
			manager.Add(new Animation(_ => GC.KeepAlive(payload), duration: 10));
			manager.Dispose();
			return new(payload);
		}

		static void CollectGarbage()
		{
			for (int i = 0; i < 3; i++)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
			}
		}

		class TestTicker : ITicker, IDisposable
		{
			public bool IsRunning { get; private set; }

			public bool SystemEnabled { get; set; } = true;

			public int MaxFps { get; set; } = 60;

			public Action Fire { get; set; }

			public int DisposeCount { get; private set; }

			public int StopCount { get; private set; }

			public void Start()
			{
				IsRunning = true;
			}

			public void Stop()
			{
				StopCount++;
				IsRunning = false;
			}

			public void Dispose()
			{
				DisposeCount++;
			}
		}
	}
}

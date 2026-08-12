using System.Threading.Tasks;
using Microsoft.Maui.Animations;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class AnimationTests : BaseTestFixture
	{
		[Fact]
		public void DisposingNonStartingManagerReleasesInsertedCallback()
		{
			var initialTweenerCount = AnimationExtensions.TweenersCounter;
			var payload = CreateDisposedManagerPayload();

			CollectGarbage();

			Assert.False(payload.IsAlive);
			Assert.Equal(initialTweenerCount, AnimationExtensions.TweenersCounter);
		}

		[Fact]
		public void DisposingNonStartingManagerReleasesAddedCallback()
		{
			var initialTweenerCount = AnimationExtensions.TweenersCounter;
			var payload = CreateDisposedManagerPayloadViaAdd();

			CollectGarbage();

			Assert.False(payload.IsAlive);
			Assert.Equal(initialTweenerCount, AnimationExtensions.TweenersCounter);
		}

		[Fact]
		public void DisposingManagerFinishesReentrantlyInsertedAnimation()
		{
			var initialTweenerCount = AnimationExtensions.TweenersCounter;
			var reentered = false;

			using (var manager = new AnimationManager(new Ticker()) { AutoStartTicker = false })
			{
				AnimationExtensions.Insert(manager, ticks =>
				{
					if (ticks == long.MaxValue && !reentered)
					{
						reentered = true;

						// Simulate a Finished/step handler that reacts to force-finish by
						// queuing another animation on the same (already disposing) manager.
						AnimationExtensions.Insert(manager, _ => true);
					}

					return true;
				});
			}

			Assert.True(reentered);
			Assert.Equal(initialTweenerCount, AnimationExtensions.TweenersCounter);
		}

		[Fact]
		public void DisposingManagerCompletesWhenCallbackThrows()
		{
			var initialTweenerCount = AnimationExtensions.TweenersCounter;
			var ticker = new DisposableTicker();
			var secondAnimationFinished = false;

			using (var manager = new AnimationManager(ticker) { AutoStartTicker = false })
			{
				AnimationExtensions.Insert(manager, _ => throw new InvalidOperationException("Simulated failure in a Finished/step callback."));
				AnimationExtensions.Insert(manager, _ =>
				{
					secondAnimationFinished = true;
					return true;
				});
			}

			// Disposal must not throw, must still finish/clean up every animation
			// (not just the ones before the throwing callback), and must still
			// dispose the ticker.
			Assert.True(secondAnimationFinished);
			Assert.True(ticker.WasDisposed);
			Assert.Equal(initialTweenerCount, AnimationExtensions.TweenersCounter);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateDisposedManagerPayload()
		{
			var payload = new object();
			var payloadReference = new WeakReference(payload);

			using (var manager = new AnimationManager(new Ticker()) { AutoStartTicker = false })
			{
				AnimationExtensions.Insert(manager, _ =>
				{
					GC.KeepAlive(payload);
					return true;
				});
			}

			return payloadReference;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateDisposedManagerPayloadViaAdd()
		{
			var payload = new object();
			var payloadReference = new WeakReference(payload);

			using (var manager = new AnimationManager(new Ticker()) { AutoStartTicker = false })
			{
				AnimationExtensions.Add(manager, _ =>
				{
					GC.KeepAlive(payload);
				});
			}

			return payloadReference;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static void CollectGarbage()
		{
			for (var iteration = 0; iteration < 3; iteration++)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
			}
		}

		sealed class DisposableTicker : Ticker, IDisposable
		{
			public bool WasDisposed { get; private set; }

			public void Dispose() => WasDisposed = true;
		}

		[Fact]
		//https://bugzilla.xamarin.com/show_bug.cgi?id=51424
		public async Task AnimationRepeats()
		{
			var box = AnimationReadyHandler.Prepare(new BoxView());
			Assert.Equal(0d, box.Rotation);
			var sb = new Animation();
			var animcount = 0;
			var rot45 = new Animation(d =>
			{
				box.Rotation = d;
				if (d > 44)
					animcount++;
			}, box.Rotation, box.Rotation + 45);
			sb.Add(0, .5, rot45);
			Assert.Equal(0d, box.Rotation);

			var i = 0;
			sb.Commit(box, "foo", length: 100, repeat: () => ++i < 2);

			await Task.Delay(1000);
			Assert.Equal(2, animcount);
		}
	}
}
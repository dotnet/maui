using System.Threading.Tasks;
using Microsoft.Maui.Animations;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class AnimationTests : BaseTestFixture
	{
		[Fact]
		public void InsertWithDisabledTickerDoesNotRetainCallback()
		{
			var initialTweenerCount = AnimationExtensions.TweenersCounter;
			using var manager = new AnimationManager(new DisabledTicker());

			AnimationExtensions.Insert(manager, _ => true);

			Assert.Equal(initialTweenerCount, AnimationExtensions.TweenersCounter);
		}

		[Fact]
		public void InsertWithDisposedManagerDoesNotRetainCallback()
		{
			var initialTweenerCount = AnimationExtensions.TweenersCounter;
			var manager = new AnimationManager(new Ticker()) { AutoStartTicker = false };
			manager.Dispose();

			AnimationExtensions.Insert(manager, _ => true);

			Assert.Equal(initialTweenerCount, AnimationExtensions.TweenersCounter);
		}

		[Fact]
		public void DisposingNonStartingManagerReleasesInsertedCallback()
		{
			var initialTweenerCount = AnimationExtensions.TweenersCounter;
			var payload = CreateDisposedManagerPayload();

			CollectGarbage();

			Assert.False(payload.IsAlive);
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
		static void CollectGarbage()
		{
			for (var iteration = 0; iteration < 3; iteration++)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
			}
		}

		sealed class DisabledTicker : Ticker
		{
			public DisabledTicker()
			{
				SystemEnabled = false;
			}
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
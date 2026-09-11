using System.Threading.Tasks;
using Microsoft.Maui.Animations;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class AnimationTests : BaseTestFixture
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void AnimationWithDisabledTickerDoesNotRetainCallback(bool useAdd)
		{
			using var manager = new AnimationManager(new DisabledTicker());

			var id = AddAnimation(manager, useAdd, null);

			Assert.False(AnimationExtensions.HasTweener(id));
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void AnimationWithDisposedManagerDoesNotRetainCallback(bool useAdd)
		{
			var manager = new AnimationManager(new Ticker()) { AutoStartTicker = false };
			manager.Dispose();

			var id = AddAnimation(manager, useAdd, null);

			Assert.False(AnimationExtensions.HasTweener(id));
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void DisposingNonStartingManagerReleasesCallback(bool useAdd)
		{
			var (manager, id, payload) = CreateDisposedManagerPayload(useAdd);

			CollectGarbage();

			Assert.False(payload.IsAlive);
			Assert.False(AnimationExtensions.HasTweener(id));
			GC.KeepAlive(manager);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static (AnimationManager Manager, int Id, WeakReference Payload) CreateDisposedManagerPayload(bool useAdd)
		{
			var payload = new object();
			var payloadReference = new WeakReference(payload);
			using var manager = new AnimationManager(new Ticker()) { AutoStartTicker = false };

			var id = AddAnimation(manager, useAdd, payload);

			return (manager, id, payloadReference);
		}

		static int AddAnimation(
			IAnimationManager manager,
			bool useAdd,
			object payload)
		{
			if (useAdd)
			{
				return AnimationExtensions.Add(manager, _ =>
				{
					GC.KeepAlive(payload);
				});
			}

			return AnimationExtensions.Insert(manager, _ =>
			{
				GC.KeepAlive(payload);
				return true;
			});
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
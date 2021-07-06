using System.Threading.Tasks;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class TickerSystemEnabledTests
	{
		[OneTimeSetUp]
		public void Init()
		{
			Device.PlatformServices = new MockPlatformServices();
		}

		[OneTimeTearDown]
		public void End()
		{
			Device.PlatformServices = null;
		}

		async Task SwapFadeViews(View view1, View view2)
		{
			await view1.FadeTo(0, 1000);
			await view2.FadeTo(1, 1000);
		}

		[Test, Timeout(3000), Ignore("https://github.com/dotnet/maui/pull/1511")]
		public async Task DisablingTickerFinishesAnimationInProgress()
		{
			var view = AnimationReadyWindowAsync.Prepare(new View { Opacity = 1 }, out var window);

			await Task.WhenAll(view.FadeTo(0, 2000), window.DisableTicker());

			Assert.That(view.Opacity, Is.EqualTo(0));
		}

		[Test, Timeout(3000), Ignore("https://github.com/dotnet/maui/pull/1511")]
		public async Task DisablingTickerFinishesAllAnimationsInChain()
		{
			var view1 = new View { Opacity = 1 };
			var view2 = new View { Opacity = 0 };

			var window = new AnimationReadyWindowAsync(view1, view2);

			await Task.WhenAll(SwapFadeViews(view1, view2), window.DisableTicker());

			Assert.That(view1.Opacity, Is.EqualTo(0));
		}

		static Task<bool> RepeatFade(View view)
		{
			var tcs = new TaskCompletionSource<bool>();
			var fadeIn = new Animation(d => { view.Opacity = d; }, 0, 1);
			var i = 0;

			fadeIn.Commit(view, "fadeIn", length: 2000, repeat: () => ++i < 2, finished: (d, b) =>
			{
				tcs.SetResult(b);
			});

			return tcs.Task;
		}

		[Test, Timeout(3000), Ignore("https://github.com/dotnet/maui/pull/1511")]
		public async Task DisablingTickerPreventsAnimationFromRepeating()
		{
			var view = AnimationReadyWindowAsync.Prepare(new View { Opacity = 0 }, out var window);

			await Task.WhenAll(RepeatFade(view), window.DisableTicker());

			Assert.That(view.Opacity, Is.EqualTo(1));
		}

		[Test]
		public async Task NewAnimationsFinishImmediatelyWhenTickerDisabled()
		{
			var view = AnimationReadyWindowAsync.Prepare(new View(), out var window);

			await window.DisableTicker();

			await view.RotateYTo(200);

			Assert.That(view.RotationY, Is.EqualTo(200));
		}

		[Test]
		public async Task AnimationExtensionsReturnTrueIfAnimationsDisabled()
		{
			var window = new AnimationReadyWindowAsync();

			await window.DisableTicker();

			var label = window.SetChild(new Label { Text = "Foo" });

			var result = await label.ScaleTo(2, 500);

			Assert.That(result, Is.True);
		}

		[Test, Timeout(2000)]
		public async Task CanExitAnimationLoopIfAnimationsDisabled()
		{
			var window = new AnimationReadyWindowAsync();

			await window.DisableTicker();

			var run = true;

			var label = window.SetChild(new Label { Text = "Foo" });

			while (run)
			{
				await label.ScaleTo(2, 500);
				run = !(await label.ScaleTo(0.5, 500));
			}
		}
	}
}
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class TickerSystemEnabledTests
	{
		[OneTimeSetUp]
		public void Init()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[OneTimeTearDown]
		public void End()
		{
			DispatcherProvider.SetCurrent(null);
		}

		async Task SwapFadeViews(View view1, View view2)
		{
			await view1.FadeTo(0, 1000);
			await view2.FadeTo(1, 1000);
		}

		[Test, Timeout(3000), Ignore("https://github.com/dotnet/maui/pull/1511")]
		public async Task DisablingTickerFinishesAnimationInProgress()
		{
			var view = AnimationReadyHandlerAsync.Prepare(new View { Opacity = 1 }, out var handler);

			await Task.WhenAll(view.FadeTo(0, 2000), handler.DisableTicker());

			Assert.That(view.Opacity, Is.EqualTo(0));
		}

		[Test, Timeout(3000), Ignore("https://github.com/dotnet/maui/pull/1511")]
		public async Task DisablingTickerFinishesAllAnimationsInChain()
		{
			var view1 = new View { Opacity = 1 };
			var view2 = new View { Opacity = 0 };

			var handler = AnimationReadyHandlerAsync.Prepare(view1, view2);

			await Task.WhenAll(SwapFadeViews(view1, view2), handler.DisableTicker());

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
			var view = AnimationReadyHandlerAsync.Prepare(new View { Opacity = 0 }, out var handler);

			await Task.WhenAll(RepeatFade(view), handler.DisableTicker());

			Assert.That(view.Opacity, Is.EqualTo(1));
		}

		[Test]
		public async Task NewAnimationsFinishImmediatelyWhenTickerDisabled()
		{
			var view = AnimationReadyHandlerAsync.Prepare(new View(), out var handler);

			await handler.DisableTicker();

			await view.RotateYTo(200);

			Assert.That(view.RotationY, Is.EqualTo(200));
		}

		[Test]
		public async Task AnimationExtensionsReturnTrueIfAnimationsDisabled()
		{
			var label = AnimationReadyHandlerAsync.Prepare(new Label { Text = "Foo" }, out var handler);

			await handler.DisableTicker();

			var result = await label.ScaleTo(2, 500);

			Assert.That(result, Is.True);
		}

		[Test, Timeout(2000)]
		public async Task CanExitAnimationLoopIfAnimationsDisabled()
		{
			var label = AnimationReadyHandlerAsync.Prepare(new Label { Text = "Foo" }, out var handler);

			await handler.DisableTicker();

			var run = true;

			while (run)
			{
				await label.ScaleTo(2, 500);
				run = !(await label.ScaleTo(0.5, 500));
			}
		}
	}
}
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class TickerSystemEnabledTests : IDisposable
	{
		public TickerSystemEnabledTests()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			DispatcherProvider.SetCurrent(null);
		}

		async Task SwapFadeViews(View view1, View view2)
		{
			await view1.FadeTo(0, 1000);
			await view2.FadeTo(1, 1000);
		}

		[Fact(Skip = "https://github.com/dotnet/maui/pull/1511", Timeout = 3000)]
		public async Task DisablingTickerFinishesAnimationInProgress()
		{
			var view = AnimationReadyHandlerAsync.Prepare(new View { Opacity = 1 }, out var handler);

			await Task.WhenAll(view.FadeTo(0, 2000), handler.DisableTicker());

			Assert.Equal(0, view.Opacity);
		}

		[Fact(Timeout = 3000, Skip = "https://github.com/dotnet/maui/pull/1511")]
		public async Task DisablingTickerFinishesAllAnimationsInChain()
		{
			var view1 = new View { Opacity = 1 };
			var view2 = new View { Opacity = 0 };

			var handler = AnimationReadyHandlerAsync.Prepare(view1, view2);

			await Task.WhenAll(SwapFadeViews(view1, view2), handler.DisableTicker());

			Assert.Equal(0, view1.Opacity);
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

		[Fact(Timeout = 3000, Skip = "https://github.com/dotnet/maui/pull/1511")]
		public async Task DisablingTickerPreventsAnimationFromRepeating()
		{
			var view = AnimationReadyHandlerAsync.Prepare(new View { Opacity = 0 }, out var handler);

			await Task.WhenAll(RepeatFade(view), handler.DisableTicker());

			Assert.Equal(1, view.Opacity);
		}

		[Fact]
		public async Task NewAnimationsFinishImmediatelyWhenTickerDisabled()
		{
			var view = AnimationReadyHandlerAsync.Prepare(new View(), out var handler);

			await handler.DisableTicker();

			await view.RotateYTo(200);

			Assert.Equal(200, view.RotationY);
		}

		[Fact]
		public async Task AnimationExtensionsReturnTrueIfAnimationsDisabled()
		{
			var label = AnimationReadyHandlerAsync.Prepare(new Label { Text = "Foo" }, out var handler);

			await handler.DisableTicker();

			var result = await label.ScaleTo(2, 500);

			Assert.True(result);
		}

		[Fact(Timeout = 2000)]
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
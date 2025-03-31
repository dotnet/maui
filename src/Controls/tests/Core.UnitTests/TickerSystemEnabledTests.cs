﻿using System;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class TickerSystemEnabledTests : IDisposable
	{
		public TickerSystemEnabledTests(ITestOutputHelper testOutput)
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			DispatcherProvider.SetCurrent(null);
		}

		static async Task SwapFadeViews(View view1, View view2)
		{
			await view1.FadeToAsync(0, 15000);
			await view2.FadeToAsync(1, 15000);
		}

		[Fact(Timeout = 3000)]
		public async Task DisablingTickerFinishesAnimationInProgress()
		{
			await SingleThreadSimulator.Run(async () =>
			{
				var view = AnimationReadyHandlerAsync.Prepare(new View { Opacity = 1 }, out var handler);

				await Task.WhenAll(view.FadeToAsync(0, 2000), handler.DisableTicker());

				Assert.Equal(0, view.Opacity);
			});
		}

		[Fact(Timeout = 3000)]
		public async Task DisablingTickerFinishesAllAnimationsInChain()
		{
			await SingleThreadSimulator.Run(async () =>
			{
				var view1 = new View { Opacity = 1 };
				var view2 = new View { Opacity = 0 };

				var handler = AnimationReadyHandlerAsync.Prepare(view1, view2);

				await Task.WhenAll(SwapFadeViews(view1, view2), handler.DisableTicker());

				Assert.Equal(0, view1.Opacity);
				Assert.Equal(1, view2.Opacity);
			});
		}

		static Task<bool> RepeatFade(View view)
		{
			var tcs = new TaskCompletionSource<bool>();
			var fadeIn = new Animation(d => { view.Opacity = d; }, 0, 1);
			var i = 0;

			fadeIn.Commit(view, "fadeIn", length: 1000, repeat: () => ++i < 5,
			finished: (d, b) =>
			{
				if ((b || i >= 5) && !tcs.Task.IsCompleted)
				{
					tcs.SetResult(b);
				}
			});

			return tcs.Task;
		}

		[Fact(Timeout = 2000)]
		public async Task DisablingTickerPreventsAnimationFromRepeating()
		{
			await SingleThreadSimulator.Run(async () =>
			{
				var view = AnimationReadyHandlerAsync.Prepare(new View { Opacity = 0 }, out var handler);

				// RepeatFade is set to repeat a 1-second animation 5 times; if it runs all the way through,
				// this test will timeout before it's finished. But disabling the ticker should cause it to
				// finish immediately, so it'll be done before the test times out.
				await Task.WhenAll(RepeatFade(view), handler.DisableTicker());

				Assert.Equal(1, view.Opacity);
			});
		}

		[Fact(Timeout = 2000)]
		public async Task NewAnimationsFinishImmediatelyWhenTickerDisabled()
		{
			await SingleThreadSimulator.Run(async () =>
			{
				var view = AnimationReadyHandlerAsync.Prepare(new View(), out var handler);

				await handler.DisableTicker();

				await view.RotateYToAsync(200);

				Assert.Equal(200, view.RotationY);
			});
		}

		[Fact(Timeout = 2000)]
		public async Task AnimationExtensionsReturnTrueIfAnimationsDisabled()
		{
			await SingleThreadSimulator.Run(async () =>
			{
				var label = AnimationReadyHandlerAsync.Prepare(new Label { Text = "Foo" }, out var handler);

				await handler.DisableTicker();

				var result = await label.ScaleToAsync(2, 500);

				Assert.True(result);
			});
		}

		[Fact(Timeout = 2000)]
		public async Task CanExitAnimationLoopIfAnimationsDisabled()
		{
			await SingleThreadSimulator.Run(async () =>
			{
				var label = AnimationReadyHandlerAsync.Prepare(new Label { Text = "Foo" }, out var handler);

				await handler.DisableTicker();

				var run = true;

				while (run)
				{
					await label.ScaleToAsync(2, 500);
					run = !(await label.ScaleToAsync(0.5, 500));
				}
			});
		}
	}
}
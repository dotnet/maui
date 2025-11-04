#nullable enable
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ButtonTests
	{
		UIButton GetPlatformButton(ButtonHandler buttonHandler) =>
			(UIButton)buttonHandler.PlatformView;

		Task<string?> GetPlatformText(ButtonHandler buttonHandler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformButton(buttonHandler).CurrentTitle);
		}

		UILineBreakMode GetPlatformLineBreakMode(ButtonHandler buttonHandler) =>
			GetPlatformButton(buttonHandler).TitleLabel.LineBreakMode;

		Task<bool> GetPlatformIsVisible(ButtonHandler buttonHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformButton(buttonHandler);
				return !nativeView.Hidden;
			});
		}

		[Fact("Clicked works after GC")]
		public async Task ClickedWorksAfterGC()
		{
			bool fired = false;
			var button = new Button();
			button.Clicked += (sender, e) => fired = true;
			var handler = await CreateHandlerAsync<ButtonHandler>(button);

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			await InvokeOnMainThreadAsync(() => handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchUpInside));
			Assert.True(fired, "Button.Clicked did not fire!");
		}

		[Theory("Button with image lays out correctly")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ButtonWithImage(bool includeText)
		{
			var gridHeight = 300;
			var gridWidth = 300;

			var button = new Button { BackgroundColor = Colors.Yellow, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
			if (includeText)
			{
				button.Text = "Hello world!";
			}

			var layout = new Grid() { HeightRequest = gridHeight, WidthRequest = gridWidth, BackgroundColor = Colors.Blue };
			layout.Add(button);

			var buttonHandler = await CreateHandlerAsync<ButtonHandler>(button);
			var handler = await CreateHandlerAsync<LayoutHandler>(layout);

			await InvokeOnMainThreadAsync(async () =>
			{
				button.ImageSource = ImageSource.FromFile("red.png");

				// Wait for image to load and force the grid to measure itself again
				await Task.Delay(1000);

				// The layout and buttons are not connected to a window, so the measure invalidation won't propagate.
				// Therefore, we have to invalidate and measure the layout manually.
				layout.InvalidateMeasure();
				layout.Measure(double.PositiveInfinity, double.PositiveInfinity);

				await handler.ToPlatform().AssertContainsColor(Colors.Blue, MauiContext); // Grid renders
				await handler.ToPlatform().AssertContainsColor(Colors.Red, MauiContext); // Image within button renders
				await handler.ToPlatform().AssertContainsColor(Colors.Yellow, MauiContext); // Button renders
			});

			// red.png is 100x100
			Assert.True(button.Width > 100, $"Button should have larger width than its image. Exepected: 100>, was {button.Width}");
			Assert.True(button.Height > 100, $"Button should have larger height than its image. Exepected: 100>, was {button.Height}");

			Assert.True(button.Width < gridWidth, $"Button shouldn't occupy entire layout width. Expected: {gridWidth}<, was {button.Width}");
			Assert.True(button.Height < gridHeight, $"Button shouldn't occupy entire layout height. Expected: {gridHeight}<, was {button.Height}");
		}

		[Fact]
		[Description("The CornerRadius of a Button should match with native CornerRadius")]
		public async Task ButtonCornerRadius()
		{
			var button = new Button
			{
				CornerRadius = 15,
			};
			var expectedValue = button.CornerRadius;

			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			var nativeView = GetPlatformButton(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				var platformCornerRadius = nativeView.Layer.CornerRadius;
				Assert.Equal(expectedValue, platformCornerRadius);
			});
		}
	}
}

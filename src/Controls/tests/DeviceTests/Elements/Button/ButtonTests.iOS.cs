#nullable enable
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
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

		Task<(string? Value, double CharacterSpacing)> GetPlatformAttributedText(ButtonHandler buttonHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var attributedTitle = GetPlatformButton(buttonHandler).CurrentAttributedTitle;

				return (attributedTitle?.Value, attributedTitle?.GetCharacterSpacing() ?? 0);
			});
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

		[Theory]
		[InlineData(false, TextTransform.Default, "Initial", "Updated")]
		[InlineData(true, TextTransform.Default, "Initial", "Updated")]
		[InlineData(false, TextTransform.Uppercase, "INITIAL", "UPDATED")]
		[Description("The Text property of a Button should update the native attributed title when CharacterSpacing is applied")]
		public async Task TextUpdatesAttributedTitleWhenCharacterSpacingApplied(bool useContentLayout, TextTransform textTransform, string initialText, string updatedText)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Button, ButtonHandler>();
				});
			});

			var button = new Button
			{
				Text = "Initial",
				CharacterSpacing = 15,
				TextTransform = textTransform
			};

			if (useContentLayout)
				button.ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Right, 8);

			await CreateHandlerAndAddToWindow(button, async () =>
			{
				var handler = Assert.IsType<ButtonHandler>(button.Handler);
				var initialAttributedText = await GetPlatformAttributedText(handler);

				Assert.Equal(initialText, initialAttributedText.Value);
				Assert.Equal(button.CharacterSpacing, initialAttributedText.CharacterSpacing);

				button.Text = "Updated";
				var updatedAttributedText = await GetPlatformAttributedText(handler);

				Assert.Equal(updatedText, updatedAttributedText.Value);
				Assert.Equal(button.CharacterSpacing, updatedAttributedText.CharacterSpacing);
			});
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

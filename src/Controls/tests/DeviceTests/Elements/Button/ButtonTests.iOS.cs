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
#if IOS && !MACCATALYST
		[Fact]
		public async Task NativeAlignmentIsNotRewrittenWhenUnset()
		{
			var button = new Button { Text = "Initial title" };
			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			await InvokeOnMainThreadAsync(() =>
			{
				var platform = handler.PlatformView;
				platform.HorizontalAlignment = UIControlContentHorizontalAlignment.Trailing;
				platform.VerticalAlignment = UIControlContentVerticalAlignment.Bottom;
				platform.TitleLabel.TextAlignment = UITextAlignment.Right;
				button.Text = "Updated title";
				Assert.Equal(UIControlContentHorizontalAlignment.Trailing, platform.HorizontalAlignment);
				Assert.Equal(UIControlContentVerticalAlignment.Bottom, platform.VerticalAlignment);
				Assert.Equal(UITextAlignment.Right, platform.TitleLabel.TextAlignment);
			});
		}

		[Theory]
		[InlineData(TextAlignment.Start)]
		[InlineData(TextAlignment.Center)]
		[InlineData(TextAlignment.End)]
		public async Task NativeTextAlignmentCanBeChangedAndCleared(TextAlignment alignment)
		{
			var button = new Button { Text = "First line\nSecond line", FlowDirection = FlowDirection.LeftToRight };
			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			await InvokeOnMainThreadAsync(() =>
			{
				var platform = handler.PlatformView;
				var originalHorizontal = platform.HorizontalAlignment;
				var originalVertical = platform.VerticalAlignment;
				var originalTitle = platform.TitleLabel.TextAlignment;
				var horizontal = alignment switch
				{
					TextAlignment.Start => UIControlContentHorizontalAlignment.Leading,
					TextAlignment.End => UIControlContentHorizontalAlignment.Trailing,
					_ => UIControlContentHorizontalAlignment.Center
				};
				var vertical = alignment switch
				{
					TextAlignment.Start => UIControlContentVerticalAlignment.Top,
					TextAlignment.End => UIControlContentVerticalAlignment.Bottom,
					_ => UIControlContentVerticalAlignment.Center
				};

				button.HorizontalTextAlignment = alignment;
				Assert.Equal(horizontal, platform.HorizontalAlignment);
				Assert.Equal(originalVertical, platform.VerticalAlignment);
				Assert.Equal(alignment switch
				{
					TextAlignment.Start => UITextAlignment.Left,
					TextAlignment.End => UITextAlignment.Right,
					_ => UITextAlignment.Center
				}, platform.TitleLabel.TextAlignment);
				button.VerticalTextAlignment = alignment;
				Assert.Equal(vertical, platform.VerticalAlignment);
				button.ClearValue(Button.HorizontalTextAlignmentProperty);
				Assert.Equal(originalHorizontal, platform.HorizontalAlignment);
				Assert.Equal(originalTitle, platform.TitleLabel.TextAlignment);
				Assert.Equal(vertical, platform.VerticalAlignment);

				button.Text = string.Empty;
				Assert.Equal(originalVertical, platform.VerticalAlignment);
				button.Text = "Restored title";
				Assert.Equal(vertical, platform.VerticalAlignment);
				button.ClearValue(Button.VerticalTextAlignmentProperty);
				Assert.Equal(originalVertical, platform.VerticalAlignment);
			});
		}

		[Theory]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Left, false)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Top, false)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Right, false)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Bottom, false)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Left, true)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Top, true)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Right, true)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Bottom, true)]
		public async Task ImageAndTextGroupRendersAtRequestedAlignment(Button.ButtonContentLayout.ImagePosition position, bool rtl)
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
				Text = "A\nB",
				ImageSource = "red.png",
				LineBreakMode = LineBreakMode.WordWrap,
				FontSize = 20,
				WidthRequest = 300,
				HeightRequest = 260,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				BorderWidth = 0,
				Padding = new Thickness(24, 12, 8, 20),
				FlowDirection = rtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
				ContentLayout = new Button.ButtonContentLayout(position, 8)
			};
			await CreateHandlerAndAddToWindow(button, async () =>
			{
				var platform = Assert.IsType<ButtonHandler>(button.Handler).PlatformView;
				await AssertHelpers.AssertEventually(() => platform.CurrentImage is not null && platform.Bounds.Width > 0 && platform.Bounds.Height > 0);
				foreach (var alignment in new[] { TextAlignment.Start, TextAlignment.Center, TextAlignment.End })
				{
					button.HorizontalTextAlignment = button.VerticalTextAlignment = alignment;
					var width = (double)platform.Bounds.Width;
					var height = (double)platform.Bounds.Height;
					((ICrossPlatformLayout)button).CrossPlatformArrange(new Rect(0, 0, width, height));
					platform.SetNeedsLayout();
					platform.LayoutIfNeeded();
					var image = platform.ImageView.Frame;
					var text = platform.TitleLabel.Frame;
					Assert.True(image.Width > 0 && image.Height > 0 && text.Width > 0 && text.Height > 0);
					var left = Math.Min((double)image.Left, (double)text.Left);
					var top = Math.Min((double)image.Top, (double)text.Top);
					var groupWidth = Math.Max((double)image.Right, (double)text.Right) - left;
					var groupHeight = Math.Max((double)image.Bottom, (double)text.Bottom) - top;
					var horizontal = rtl && alignment != TextAlignment.Center
						? alignment == TextAlignment.Start ? TextAlignment.End : TextAlignment.Start : alignment;
					var expectedLeft = button.Padding.Left + (width - button.Padding.HorizontalThickness - groupWidth) *
						(horizontal == TextAlignment.Start ? 0 : horizontal == TextAlignment.End ? 1 : 0.5);
					var expectedTop = button.Padding.Top + (height - button.Padding.VerticalThickness - groupHeight) *
						(alignment == TextAlignment.Start ? 0 : alignment == TextAlignment.End ? 1 : 0.5);
					Assert.InRange(Math.Abs(left - expectedLeft), 0, 1);
					Assert.InRange(Math.Abs(top - expectedTop), 0, 1);
					var gap = position switch
					{
						Button.ButtonContentLayout.ImagePosition.Top => (double)(text.Top - image.Bottom),
						Button.ButtonContentLayout.ImagePosition.Bottom => (double)(image.Top - text.Bottom),
						_ => (position == Button.ButtonContentLayout.ImagePosition.Left) != rtl
							? (double)(text.Left - image.Right) : (double)(image.Left - text.Right)
					};
					Assert.InRange(Math.Abs(gap - button.ContentLayout.Spacing), 0, 1);
				}
			});
		}
#endif

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

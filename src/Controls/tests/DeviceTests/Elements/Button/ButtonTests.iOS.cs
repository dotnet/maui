#nullable enable
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.ImageAnalysis;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ButtonTests
	{
		[Fact]
		public async Task AlignmentMapperCustomizationSurvivesArrange()
		{
			var button = new Button
			{
				Text = "Customized title",
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Start
			};
			var handler = await CreateHandlerAsync<CustomizedAlignmentButtonHandler>(button);
			await InvokeOnMainThreadAsync(() =>
			{
				((ICrossPlatformLayout)button).CrossPlatformArrange(new Rect(0, 0, 300, 100));
				Assert.Equal(UITextAlignment.Right, handler.PlatformView.TitleLabel.TextAlignment);
				button.VerticalTextAlignment = TextAlignment.End;
				Assert.Equal(UITextAlignment.Right, handler.PlatformView.TitleLabel.TextAlignment);
			});
		}

		class CustomizedAlignmentButtonHandler : ButtonHandler
		{
			public CustomizedAlignmentButtonHandler() : base(CreateMapper())
			{
			}

			static IPropertyMapper CreateMapper()
			{
				var mapper = new PropertyMapper<Button, IButtonHandler>(ButtonHandler.Mapper);
				mapper.AppendToMapping(nameof(Button.HorizontalTextAlignment), (handler, button) =>
					handler.PlatformView.TitleLabel.TextAlignment = UITextAlignment.Right);
				return mapper;
			}
		}

		class ConfiguredAlignmentButtonHandler : ButtonHandler
		{
			protected override UIButton CreatePlatformView()
			{
				var button = base.CreatePlatformView();
				button.Configuration = UIButtonConfiguration.BorderedButtonConfiguration;
				return button;
			}
		}

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
		[InlineData(TextAlignment.Justify)]
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
					TextAlignment.Justify => UIControlContentHorizontalAlignment.Fill,
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
					TextAlignment.Justify => UITextAlignment.Justified,
					TextAlignment.Start => UITextAlignment.Left,
					TextAlignment.End => UITextAlignment.Right,
					_ => UITextAlignment.Center
				}, platform.TitleLabel.TextAlignment);
				button.VerticalTextAlignment = alignment == TextAlignment.Justify ? TextAlignment.Center : alignment;
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

#endif
		[Theory]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Left, false, false)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Top, false, false)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Right, false, false)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Bottom, false, false)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Left, true, false)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Top, true, false)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Right, true, false)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Bottom, true, false)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Left, false, true)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Top, false, true)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Right, false, true)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Bottom, false, true)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Left, true, true)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Top, true, true)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Right, true, true)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Bottom, true, true)]
		public async Task ImageAndTextGroupRendersAtRequestedAlignment(Button.ButtonContentLayout.ImagePosition position, bool rtl, bool configured)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					if (configured)
					{
						handlers.AddHandler<Button, ConfiguredAlignmentButtonHandler>();
					}
					else
					{
						handlers.AddHandler<Button, ButtonHandler>();
					}
				});
			});
			var button = new Button
			{
				Text = "A\nB",
				ImageSource = "red.png",
				LineBreakMode = LineBreakMode.WordWrap,
				FontSize = 20,
				BackgroundColor = Colors.White,
				WidthRequest = 300,
				HeightRequest = 260,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				BorderWidth = 0,
				Padding = new Thickness(24, 12, 8, 20),
				FlowDirection = rtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
				TextColor = Colors.Blue,
				ContentLayout = new Button.ButtonContentLayout(position, 8)
			};
			await CreateHandlerAndAddToWindow(button, async () =>
			{
				var platform = Assert.IsAssignableFrom<ButtonHandler>(button.Handler).PlatformView;
				await AssertHelpers.AssertEventually(() => platform.CurrentImage is not null && platform.Bounds.Width > 0 && platform.Bounds.Height > 0);
				var originalConfiguration = platform.Configuration;
				Assert.True(!configured || originalConfiguration is not null);
				Rect? start = null;
				Rect? center = null;
				foreach (var alignment in new[] { TextAlignment.Start, TextAlignment.Center, TextAlignment.End })
				{
					button.HorizontalTextAlignment = alignment;
					button.VerticalTextAlignment = alignment;
					var width = (double)platform.Bounds.Width;
					var height = (double)platform.Bounds.Height;
					((ICrossPlatformLayout)button).CrossPlatformArrange(new Rect(0, 0, width, height));
					platform.SetNeedsLayout();
					platform.LayoutIfNeeded();
					var imageFrame = platform.ImageView.ConvertRectToView(platform.ImageView.Bounds, platform);
					var text = platform.TitleLabel.ConvertRectToView(platform.TitleLabel.Bounds, platform);
					for (var repeat = 0; repeat < 2; repeat++)
					{
						platform.SetNeedsLayout();
						platform.LayoutIfNeeded();
						Assert.Equal(imageFrame, platform.ImageView.ConvertRectToView(platform.ImageView.Bounds, platform));
						Assert.Equal(text, platform.TitleLabel.ConvertRectToView(platform.TitleLabel.Bounds, platform));
					}
					var bitmap = await button.AsRawBitmapAsync();
					var image = GetRenderedRedImageBounds(bitmap);
					var ink = GetRenderedBlueTextBounds(bitmap);
					var left = Math.Min(image.Left, ink.Left);
					var top = Math.Min(image.Top, ink.Top);
					var group = new Rect(left, top,
						Math.Max(image.Right, ink.Right) - left, Math.Max(image.Bottom, ink.Bottom) - top);
					if (alignment == TextAlignment.Start)
					{
						start = group;
					}
					else if (alignment == TextAlignment.Center)
					{
						center = group;
					}
					else
					{
						Assert.True(start.HasValue && center.HasValue);
						Assert.True(rtl
							? start.Value.Left > center.Value.Left + 4 && center.Value.Left > group.Left + 4
							: start.Value.Left + 4 < center.Value.Left && center.Value.Left + 4 < group.Left);
						Assert.True(start.Value.Top + 4 < center.Value.Top && center.Value.Top + 4 < group.Top);
					}
				}
				button.ClearValue(Button.HorizontalTextAlignmentProperty);
				button.ClearValue(Button.VerticalTextAlignmentProperty);
				((ICrossPlatformLayout)button).CrossPlatformArrange(new Rect(0, 0, platform.Bounds.Width, platform.Bounds.Height));
				if (originalConfiguration is not null)
				{
					Assert.Equal(originalConfiguration.ImagePlacement, platform.Configuration!.ImagePlacement);
					Assert.Equal(originalConfiguration.ImagePadding, platform.Configuration.ImagePadding);
					Assert.Equal(originalConfiguration.ContentInsets, platform.Configuration.ContentInsets);
				}
			});
		}

		static Rect GetRenderedRedImageBounds(RawBitmap bitmap)
		{
			var left = bitmap.PixelWidth;
			var top = bitmap.PixelHeight;
			var right = -1;
			var bottom = -1;
			for (var y = 0; y < bitmap.PixelHeight; y++)
			{
				for (var x = 0; x < bitmap.PixelWidth; x++)
				{
					var offset = (y * bitmap.PixelWidth + x) * 4;
					if (bitmap.PixelBuffer[offset + 2] > 180 &&
						bitmap.PixelBuffer[offset + 1] < 80 && bitmap.PixelBuffer[offset] < 80)
					{
						left = Math.Min(left, x);
						right = Math.Max(right, x);
						top = Math.Min(top, y);
						bottom = Math.Max(bottom, y);
					}
				}
			}
			Assert.True(right >= left && bottom >= top, "The button image must render.");
			return new Rect(left / bitmap.Density, top / bitmap.Density,
				(right - left + 1) / bitmap.Density, (bottom - top + 1) / bitmap.Density);
		}

		static Rect GetRenderedBlueTextBounds(RawBitmap bitmap)
		{
			var left = bitmap.PixelWidth;
			var top = bitmap.PixelHeight;
			var right = -1;
			var bottom = -1;
			for (var y = 0; y < bitmap.PixelHeight; y++)
			{
				for (var x = 0; x < bitmap.PixelWidth; x++)
				{
					var offset = (y * bitmap.PixelWidth + x) * 4;
					if (bitmap.PixelBuffer[offset] > 180 &&
						bitmap.PixelBuffer[offset + 1] < 80 && bitmap.PixelBuffer[offset + 2] < 80)
					{
						left = Math.Min(left, x);
						right = Math.Max(right, x);
						top = Math.Min(top, y);
						bottom = Math.Max(bottom, y);
					}
				}
			}
			Assert.True(right >= left && bottom >= top, "The button title must render.");
			return new Rect(left / bitmap.Density, top / bitmap.Density,
				(right - left + 1) / bitmap.Density, (bottom - top + 1) / bitmap.Density);
		}

		[Fact]
		public async Task HorizontalJustificationCanBeChangedAndCleared()
		{
			var button = new Button { Text = "Words that wrap across several lines", LineBreakMode = LineBreakMode.WordWrap };
			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			await InvokeOnMainThreadAsync(() =>
			{
				var platform = handler.PlatformView;
				var originalHorizontal = platform.HorizontalAlignment;
				var originalTitle = platform.TitleLabel.TextAlignment;
				button.HorizontalTextAlignment = TextAlignment.Justify;
				Assert.Equal(UIControlContentHorizontalAlignment.Fill, platform.HorizontalAlignment);
				Assert.Equal(UITextAlignment.Justified, platform.TitleLabel.TextAlignment);
				button.Text = string.Empty;
				Assert.Equal(originalHorizontal, platform.HorizontalAlignment);
				Assert.Equal(originalTitle, platform.TitleLabel.TextAlignment);
				button.Text = "Restored words";
				Assert.Equal(UITextAlignment.Justified, platform.TitleLabel.TextAlignment);
				button.HorizontalTextAlignment = TextAlignment.End;
				Assert.Equal(UITextAlignment.Right, platform.TitleLabel.TextAlignment);
				button.ClearValue(Button.HorizontalTextAlignmentProperty);
				Assert.Equal(originalHorizontal, platform.HorizontalAlignment);
				Assert.Equal(originalTitle, platform.TitleLabel.TextAlignment);
			});
		}

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

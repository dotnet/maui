using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.DeviceTests.ImageAnalysis;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public partial class ButtonTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData(null)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Left)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Right)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Top)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Bottom)]
		public async Task JustificationExpandsWrappedWordsButNotTheLastLine(Button.ButtonContentLayout.ImagePosition? position)
		{
#if ANDROID
			if (!OperatingSystem.IsAndroidVersionAtLeast(26))
			{
				return;
			}
#endif
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers => handlers.AddHandler<Button, ButtonHandler>()));
			var button = new Button
			{
				Text = "word word word word word word word word word word word word end",
				ImageSource = position.HasValue ? "red.png" : null,
				ContentLayout = new Button.ButtonContentLayout(position ?? Button.ButtonContentLayout.ImagePosition.Left, 8),
				TextColor = Colors.Blue,
				BackgroundColor = Colors.White,
				FontSize = 20,
				WidthRequest = 300,
				HeightRequest = 360,
				Padding = new Thickness(16),
				BorderWidth = 0,
				LineBreakMode = LineBreakMode.WordWrap,
				HorizontalTextAlignment = TextAlignment.Justify,
				VerticalTextAlignment = TextAlignment.Center
			};
			await CreateHandlerAndAddToWindow(button, async () =>
			{
#if WINDOWS
				var imageWidth = position.HasValue ? await GetJustificationImageWidth(button) : 0;
				var bitmap = await GetJustificationBitmap(button);
#else
				var bitmap = await button.AsRawBitmapAsync();
#endif
				var lines = GetBlueTextLineWidths(bitmap);
				Assert.True(lines.Count > 1, "The text must wrap naturally into multiple rendered lines.");
#if !WINDOWS
				var imageWidth = position is Button.ButtonContentLayout.ImagePosition.Left or Button.ButtonContentLayout.ImagePosition.Right
					? 100 + button.ContentLayout.Spacing : 0;
#endif
				var available = bitmap.PixelWidth - (button.Padding.HorizontalThickness + imageWidth) * bitmap.Density;
				Assert.InRange(lines[0], available - button.FontSize * bitmap.Density / 2, available);
				Assert.True(lines[lines.Count - 1] < available - button.FontSize * bitmap.Density,
					"The final line must not be stretched to the justified width.");
			});
		}

		static List<int> GetBlueTextLineWidths(RawBitmap bitmap)
		{
			var widths = new List<int>();
			var left = bitmap.PixelWidth;
			var right = -1;
			for (var y = 0; y <= bitmap.PixelHeight; y++)
			{
				bool ink = false;
				for (var x = 0; y < bitmap.PixelHeight && x < bitmap.PixelWidth; x++)
				{
					var offset = (y * bitmap.PixelWidth + x) * 4;
					if (bitmap.PixelBuffer[offset] > 180 && bitmap.PixelBuffer[offset + 1] < 80 && bitmap.PixelBuffer[offset + 2] < 80)
					{
						ink = true;
						left = Math.Min(left, x);
						right = Math.Max(right, x);
					}
				}
				if (!ink && right >= left)
				{
					widths.Add(right - left + 1);
					left = bitmap.PixelWidth;
					right = -1;
				}
			}
			return widths;
		}

		[Theory]
		[ClassData(typeof(TextTransformCases))]
		public async Task InitialTextTransformApplied(string text, TextTransform transform, string expected)
		{
			var control = new Button() { Text = text, TextTransform = transform };
			var platformText = await GetPlatformText(await CreateHandlerAsync<ButtonHandler>(control));
			Assert.Equal(expected, platformText);
		}

		[Theory]
		[ClassData(typeof(TextTransformCases))]
		public async Task TextTransformUpdated(string text, TextTransform transform, string expected)
		{
			var control = new Button() { Text = text };
			var handler = await CreateHandlerAsync<ButtonHandler>(control);
			await InvokeOnMainThreadAsync(() => control.TextTransform = transform);
			var platformText = await GetPlatformText(handler);
			Assert.Equal(expected, platformText);
		}

		[Theory(DisplayName = "LineBreakMode Initializes Correctly")]
		[InlineData(LineBreakMode.MiddleTruncation)]
		[InlineData(LineBreakMode.HeadTruncation)]
		[InlineData(LineBreakMode.TailTruncation)]
		[InlineData(LineBreakMode.WordWrap)]
		[InlineData(LineBreakMode.CharacterWrap)]
		[InlineData(LineBreakMode.NoWrap)]
		public async Task LineBreakModeInitializesCorrectly(LineBreakMode lineBreakMode)
		{
			var xplatLineBreakMode = lineBreakMode;

			var button = new Button()
			{
				LineBreakMode = xplatLineBreakMode
			};

			var expectedValue = xplatLineBreakMode.ToPlatform();

			var handler = await CreateHandlerAsync<ButtonHandler>(button);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.Equal(expectedValue, GetPlatformLineBreakMode(handler));
			});
		}

		[Fact]
		[Description("The BackgroundColor of a Button should match with native background color")]
		public async Task ButtonBackgroundColorConsistent()
		{
			var expected = Colors.AliceBlue;
			var button = new Button()
			{
				BackgroundColor = expected,
				HeightRequest = 100,
				WidthRequest = 200
			};

			await ValidateHasColor(button, expected, typeof(ButtonHandler));
		}

		[Fact]
		[Description("The IsVisible property of a Button should match with native IsVisible")]
		public async Task VerifyButtonIsVisibleProperty()
		{
			var button = new Button();
			button.IsVisible = false;
			var expectedValue = button.IsVisible;

			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			await InvokeOnMainThreadAsync(async () =>
			{
				var isVisible = await GetPlatformIsVisible(handler);
				Assert.Equal(expectedValue, isVisible);
			});
		}
	}
}
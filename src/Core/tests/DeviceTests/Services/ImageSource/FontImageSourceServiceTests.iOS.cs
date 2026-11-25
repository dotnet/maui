using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CoreGraphics;
using CoreText;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class FontImageSourceServiceTests
	{
		[Theory]
		[InlineData(typeof(FileImageSourceStub))]
		[InlineData(typeof(StreamImageSourceStub))]
		[InlineData(typeof(UriImageSourceStub))]
		public async Task ThrowsForIncorrectTypes([DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
		{
			var service = new FontImageSourceService(null);

			var imageSource = (ImageSourceStub)Activator.CreateInstance(type);

			await Assert.ThrowsAsync<InvalidCastException>(() => service.GetImageAsync(imageSource));
		}

		[Theory]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#000000")]
		public async Task GetImageAsync(string colorHex)
		{
			var expectedColor = Color.FromArgb(colorHex);

			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureFonts()
				.ConfigureImageSources()
				.Build();

			var images = mauiApp.Services.GetRequiredService<IImageSourceServiceProvider>();
			var service = images.GetRequiredImageSourceService<FontImageSourceStub>();

			var imageSource = new FontImageSourceStub
			{
				Glyph = "X",
				Font = Font.Default.WithSize(30),
				Color = expectedColor,
			};

			using var drawable = await InvokeOnMainThreadAsync(() => service.GetImageAsync(imageSource));

			var uiimage = Assert.IsType<UIImage>(drawable.Value);

			await uiimage.AssertContainsColor(expectedColor.ToPlatform()).ConfigureAwait(false);
		}

		[Fact]
		public async Task GetImageAsyncWithCustomFont()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("dokdo_regular.ttf", "Dokdo");
				})
				.ConfigureImageSources()
				.Build();

			var images = mauiApp.Services.GetRequiredService<IImageSourceServiceProvider>();
			var service = images.GetRequiredImageSourceService<FontImageSourceStub>();

			var imageSource = new FontImageSourceStub
			{
				Glyph = "X",
				Font = Font.OfSize("Dokdo", 24),
				Color = Colors.Red,
			};

			using var drawable = await InvokeOnMainThreadAsync(() => service.GetImageAsync(imageSource));

			var uiimage = Assert.IsType<UIImage>(drawable.Value);

			await uiimage.AssertContainsColor(Colors.Red.ToPlatform()).ConfigureAwait(false);
		}

		[Theory]
		[InlineData(null, "\u2039", 16, 1)]
		[InlineData(null, "\u2039", 30, 2)]
		[InlineData(null, "\u2039", 48, 3)]
		[InlineData(null, "X", 30, 3)]
		[InlineData(null, "g", 30, 3)]
		[InlineData(null, "A\u030A", 30, 3)]
		[InlineData("Dokdo", "\u2039", 30, 3)]
		public async Task GlyphIsCenteredWithoutChangingFontMetrics(string fontFamily, string glyph, double size, float scale)
		{
			using var mauiApp = MauiApp.CreateBuilder()
				.ConfigureFonts(fonts => fonts.AddFont("dokdo_regular.ttf", "Dokdo"))
				.Build();

			var fontManager = mauiApp.Services.GetRequiredService<IFontManager>();
			var imageSource = new FontImageSourceStub
			{
				Glyph = glyph,
				Font = Font.OfSize(fontFamily, size),
				Color = Colors.Red
			};

			await InvokeOnMainThreadAsync(() =>
			{
				using var image = imageSource.GetPlatformImage(fontManager, scale);
				Assert.NotNull(image);
				using var text = new NSString(glyph);
				using var attributedText = new NSAttributedString(text, fontManager.GetFont(imageSource.Font), UIColor.Red);
				var expectedSize = text.GetSizeUsingAttributes(attributedText.GetUIKitAttributes(0, out _));

				Assert.InRange(Math.Abs((double)(image.Size.Width - expectedSize.Width)), 0, 1 / (double)scale);
				Assert.InRange(Math.Abs((double)(image.Size.Height - expectedSize.Height)), 0, 1 / (double)scale);
				Assert.Equal(UIImageRenderingMode.AlwaysOriginal, image.RenderingMode);

				var ink = GetGlyphInkBounds(image);
				Assert.True(ink.Width > 0 && ink.Height > 0, "The glyph must contain visible ink.");

				if (OperatingSystem.IsIOSVersionAtLeast(26))
				{
					using var line = new CTLine(attributedText);
					var expectedInk = line.GetBounds(CTLineBoundsOptions.UseGlyphPathBounds);
					Assert.InRange(Math.Abs((double)ink.GetMidX() - image.CGImage.Width / 2d), 0, 1);
					Assert.InRange(Math.Abs((double)ink.GetMidY() - image.CGImage.Height / 2d), 0, 1);
					Assert.InRange(Math.Abs((double)ink.Width - (double)expectedInk.Width * scale), 0, 2);
					Assert.InRange(Math.Abs((double)ink.Height - (double)expectedInk.Height * scale), 0, 2);
				}
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task RenderingModeIsPreserved(bool hasColor)
		{
			using var mauiApp = MauiApp.CreateBuilder().ConfigureFonts().Build();
			var fontManager = mauiApp.Services.GetRequiredService<IFontManager>();
			var imageSource = new FontImageSourceStub
			{
				Glyph = "X",
				Font = Font.Default.WithSize(30),
				Color = hasColor ? Colors.Red : null
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				using var image = imageSource.GetPlatformImage(fontManager, 3);
				Assert.NotNull(image);
				Assert.Equal(hasColor ? UIImageRenderingMode.AlwaysOriginal : UIImageRenderingMode.Automatic, image.RenderingMode);
				await image.AssertContainsColor(hasColor ? UIColor.Red : UIColor.White);
			});
		}

		static CGRect GetGlyphInkBounds(UIImage image)
		{
			var cgImage = image.CGImage;
			var width = (int)cgImage.Width;
			var height = (int)cgImage.Height;
			var pixels = new byte[width * height * 4];
			using var colorSpace = CGColorSpace.CreateDeviceRGB();
			using var context = new CGBitmapContext(pixels, width, height, 8, width * 4, colorSpace,
				CGBitmapFlags.ByteOrder32Big | CGBitmapFlags.PremultipliedLast);
			context.DrawImage(new CGRect(0, 0, width, height), cgImage);

			var left = width;
			var top = height;
			var right = -1;
			var bottom = -1;
			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					if (pixels[(y * width + x) * 4 + 3] < 128)
						continue;

					left = Math.Min(left, x);
					top = Math.Min(top, y);
					right = Math.Max(right, x);
					bottom = Math.Max(bottom, y);
				}
			}

			return right < left ? CGRect.Empty : new CGRect(left, top, right - left + 1, bottom - top + 1);
		}
	}
}
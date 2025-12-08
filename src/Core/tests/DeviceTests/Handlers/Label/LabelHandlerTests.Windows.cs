using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Storage;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LabelHandlerTests
	{
		[Theory]
		[InlineData(true, Skip = "https://github.com/dotnet/maui/issues/26749")]
		[InlineData(false)]
		public async Task FontFamilyIsCorrectForRendering(bool validateSize)
		{
			var label = new LabelStub
			{
				Font = Font.OfSize("Ion", 20),
				Text = "\xf30c",
			};

			var (bounds, font) = await GetValueAsync(label, handler =>
			{
				var fe = handler.VirtualView.ToPlatform();
				var tb = fe as TextBlock;

				return tb.AttachAndRun(() => (tb.GetBoundingBox(), tb.FontFamily.Source), MauiContext);
			});

			var expectedFont = "ms-appx:///ionicons.ttf#Ionicons";

			Assert.Equal(expectedFont, font);

			if (validateSize)
			{
				Assert.True(bounds.Width > 20, $"Width ({bounds.Width}x{bounds.Height}) was too narrow, the font probably did not load correctly.");
				Assert.True(bounds.Height < 30, $"Height ({bounds.Width}x{bounds.Height}) was too tall, the font probably did not load correctly.");
			}
		}

		[Theory]
		[InlineData(true, Skip = "https://github.com/dotnet/maui/issues/26749")]
		[InlineData(false)]
		public async Task EmbeddedFontFamilyIsCorrectForRendering(bool validateSize)
		{
			var label = new LabelStub
			{
				Font = Font.OfSize("Embedded", 20),
				Text = "\xf30c",
			};

			var (bounds, font) = await GetValueAsync(label, handler =>
			{
				var fe = handler.VirtualView.ToPlatform();
				var tb = fe as TextBlock;

				return tb.AttachAndRun(() => (tb.GetBoundingBox(), tb.FontFamily.Source), MauiContext);
			});


#if UNPACKAGED
			var expectedSuffix = "Fonts\\ionicons.ttf#Ionicons";
			var root = Path.GetFullPath(Path.Combine(FileSystem.CacheDirectory, ".."));
			var fullFontName = Path.GetFullPath(Path.Combine(root, expectedSuffix));
			var expectedFont = "ms-appx:///" + fullFontName.Replace("\\", "/", StringComparison.OrdinalIgnoreCase);
#else
			var expectedFont = "ms-appdata:///temp/Fonts/ionicons.ttf#Ionicons";
#endif

			Assert.Equal(expectedFont, font);

			if (validateSize)
			{
				Assert.True(bounds.Width > 20, $"Width ({bounds.Width}x{bounds.Height}) was too narrow, the font probably did not load correctly.");
				Assert.True(bounds.Height < 30, $"Height ({bounds.Width}x{bounds.Height}) was too tall, the font probably did not load correctly.");
			}
		}

		TextBlock GetPlatformLabel(LabelHandler labelHandler) =>
			labelHandler.PlatformView;

		string GetNativeText(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).Text;

		Color GetNativeTextColor(LabelHandler labelHandler)
		{
			var platformLabel = GetPlatformLabel(labelHandler);

			var foreground = platformLabel.Foreground;

			if (foreground is SolidColorBrush solidColorBrush)
				return solidColorBrush.Color.ToColor();

			return null;
		}

		UI.Xaml.TextAlignment GetNativeHorizontalTextAlignment(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).TextAlignment;
	}
}
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class FontImageSourceServiceTests
	{
		[Fact]
		public Task SystemFontGlyphRenders() =>
			InvokeOnMainThreadAsync(async () =>
			{
				var manager = new FontManager(new FontRegistrar(fontLoader: null));
				var service = new FontImageSourceService(manager);
				var source = new FontImageSourceStub
				{
					Glyph = "\uE711",
					Font = Font.OfSize("Segoe MDL2 Assets", 48),
					Color = Colors.Red
				};

				using var result = await service.GetImageSourceAsync(source);
				var canvas = Assert.IsType<CanvasImageSource>(result.Value);
				var image = new Image { Source = canvas, Width = 60, Height = 60 };
				using var services = new ServiceCollection().BuildServiceProvider();
				using var bitmap = await image.AssertContainsColor(Colors.Red, new ContextStub(services));
			});

		[Theory]
		[InlineData("Assets/Fonts/insubfolder.ttf#Dokdo")]
		[InlineData("ms-appx:///Assets/Fonts/insubfolder.ttf#Dokdo")]
		public Task FontFileSourceResolvesForCurrentPackaging(string fontSource) =>
			InvokeOnMainThreadAsync(() =>
			{
				var service = new FontImageSourceService(new FixedFontManager(fontSource));
				var source = new FontImageSourceStub { Font = Font.OfSize("Dokdo", 12) };

#if UNPACKAGED
				var path = Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts", "insubfolder.ttf");
				Assert.True(File.Exists(path), $"File not found: {path}");
				var expected = "file:///" + path.Replace('\\', '/') + "#Dokdo";
#else
				var expected = fontSource;
#endif
				Assert.Equal(expected, service.GetFontSource(source));
			});

		[Theory]
		[InlineData("Assets/Fonts/missing-font.ttf#Missing")]
		[InlineData("ms-appx:///Assets/Fonts/missing-font.ttf#Missing")]
		[InlineData("Segoe MDL2 Assets")]
		public Task UnresolvedFontSourceIsPreserved(string fontSource) =>
			InvokeOnMainThreadAsync(() =>
			{
				var service = new FontImageSourceService(new FixedFontManager(fontSource));
				var source = new FontImageSourceStub { Font = Font.OfSize("Missing", 12) };

				Assert.Equal(fontSource, service.GetFontSource(source));
			});

		sealed class FixedFontManager : IFontManager
		{
			readonly FontFamily _family;

			public FixedFontManager(string source) => _family = new FontFamily(source);

			public FontFamily DefaultFontFamily => _family;
			public double DefaultFontSize => 12;
			public FontFamily GetFontFamily(Font font) => _family;
			public double GetFontSize(Font font, double defaultFontSize = 0) => font.Size;
		}
	}
}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class FontImageSourceServiceTests
	{
		[Theory]
		[InlineData(null, "A")]
		[InlineData("Segoe UI", "A")]
		[InlineData("Segoe MDL2 Assets", "\uE711")]
		public Task SystemFontImageHasExpectedGlyphBounds(string fontFamily, string glyph) =>
			InvokeOnMainThreadAsync(async () =>
			{
				const float fontSize = 30;
				var manager = new FontManager(new FontRegistrar(fontLoader: null));
				var service = new FontImageSourceService(manager);
				var imageSource = new FontImageSourceStub
				{
					Font = Font.OfSize(fontFamily, fontSize),
					Glyph = glyph,
					Color = Colors.Red
				};

				using var result = await service.GetImageSourceAsync(imageSource);
				Assert.NotNull(result);
				var image = Assert.IsType<CanvasImageSource>(result.Value);

				AssertGlyphBounds(image, fontFamily ?? manager.DefaultFontFamily.Source.Split(',')[0], glyph, fontSize);
			});

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public Task RegisteredFontImageHasExpectedGlyphBounds(bool embedded) =>
			InvokeOnMainThreadAsync(async () =>
			{
				const float fontSize = 60;
				const string glyph = "M";
				var registrar = new FontRegistrar(new EmbeddedFontLoader());
				if (embedded)
					registrar.Register("dokdo_regular.ttf", "Dokdo", typeof(FontImageSourceServiceTests).Assembly);
				else
					registrar.Register("dokdo_regular.ttf", "Dokdo");

				var service = new FontImageSourceService(new FontManager(registrar));
				var imageSource = new FontImageSourceStub
				{
					Font = Font.OfSize("Dokdo", fontSize),
					Glyph = glyph,
					Color = Colors.Red
				};

				using var result = await service.GetImageSourceAsync(imageSource);
				Assert.NotNull(result);
				var image = Assert.IsType<CanvasImageSource>(result.Value);

#if UNPACKAGED
				var path = embedded
					? Path.GetFullPath(Path.Combine(FileSystem.CacheDirectory, "..", "Fonts", "dokdo_regular.ttf"))
					: Path.Combine(AppContext.BaseDirectory, "dokdo_regular.ttf");
				var expectedSource = $"file:///{path.Replace('\\', '/')}#Dokdo";
#else
				var expectedSource = embedded
					? "ms-appdata:///temp/Fonts/dokdo_regular.ttf#Dokdo"
					: "ms-appx:///dokdo_regular.ttf#Dokdo";
#endif

				AssertGlyphBounds(image, expectedSource, glyph, fontSize);
			});

		[Theory]
		[InlineData("Segoe MDL2 Assets")]
		[InlineData("Segoe UI")]
		[InlineData("System font: alternative")]
		[InlineData("Assets/Fonts/missing-font.ttf#Missing Font")]
		[InlineData("ms-appx:///missing-font.ttf#Missing Font")]
		public void UnpackagedFontSourcePreservesFamilyNamesAndMissingFiles(string source) =>
			Assert.Equal(source, FontImageSourceService.GetUnpackagedFontSource(source));

		[Theory]
		[InlineData("dokdo_regular.ttf#Dokdo")]
		[InlineData("dokdo%5Fregular.ttf#Dokdo")]
		[InlineData("ms-appx:///dokdo_regular.ttf#Dokdo")]
		public void UnpackagedFontSourceResolvesFontFilesAndPreservesFamily(string source)
		{
			var actual = FontImageSourceService.GetUnpackagedFontSource(source);

#if UNPACKAGED
			var path = Path.Combine(AppContext.BaseDirectory, "dokdo_regular.ttf").Replace('\\', '/');
			var expected = $"file:///{path}#Dokdo";
#else
			var expected = "ms-appx:///dokdo_regular.ttf#Dokdo";
#endif

			Assert.Equal(expected, actual);
		}

		static void AssertGlyphBounds(CanvasImageSource image, string fontFamily, string glyph, float fontSize)
		{
			using var textFormat = new CanvasTextFormat
			{
				FontFamily = fontFamily,
				FontSize = fontSize,
				HorizontalAlignment = CanvasHorizontalAlignment.Center,
				VerticalAlignment = CanvasVerticalAlignment.Center,
				Options = CanvasDrawTextOptions.Default
			};
			using var layout = new CanvasTextLayout(CanvasDevice.GetSharedDevice(), glyph, textFormat, 0, 0);

			Assert.True(layout.DrawBounds.Width > 0);
			Assert.True(layout.DrawBounds.Height > 0);
			Assert.InRange(Math.Abs(image.Size.Width - (layout.DrawBounds.Width + 2)), 0, 1);
			Assert.InRange(Math.Abs(image.Size.Height - (layout.DrawBounds.Height + 2)), 0, 1);
		}
	}
}

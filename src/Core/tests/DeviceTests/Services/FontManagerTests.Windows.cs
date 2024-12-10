using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

public partial class FontManagerTests : TestBase
{
	[Theory]
	[InlineData("dokdo_regular", "ms-appx:///dokdo_regular.ttf#Dokdo")]
	[InlineData("dokdo_regular.ttf", "ms-appx:///dokdo_regular.ttf#Dokdo")]
	[InlineData("dokdo_regular#dokdo", "ms-appx:///dokdo_regular.ttf#Dokdo")]
	[InlineData("dokdo_regular.ttf#dokdo", "ms-appx:///dokdo_regular.ttf#Dokdo")]
	[InlineData("myalias", "ms-appx:///dokdo_regular.ttf#Dokdo")]
	public Task CanLoadFonts(string fontName, string actualFontFamily) =>
		InvokeOnMainThreadAsync(() =>
		{
			var registrar = new FontRegistrar(fontLoader: null);
			registrar.Register("dokdo_regular.ttf", "myalias");
			var manager = new FontManager(registrar);

			var actual = manager.GetFontFamily(Font.OfSize(fontName, 12, FontWeight.Regular));

			Assert.Equal(actualFontFamily, actual.Source);
		});

	[Theory]
	[InlineData("dokdo_regular")]
	[InlineData("dokdo_regular.ttf")]
	[InlineData("dokdo_regular#dokdo")]
	[InlineData("dokdo_regular.ttf#dokdo")]
	[InlineData("myalias")]
	public Task CanLoadEmbeddedFont(string fontName) =>
		InvokeOnMainThreadAsync(() =>
		{
			var registrar = new FontRegistrar(new EmbeddedFontLoader());
			registrar.Register("dokdo_regular.ttf", "myalias", GetType().Assembly);
			var manager = new FontManager(registrar);

			var actual = manager.GetFontFamily(Font.OfSize(fontName, 12, FontWeight.Regular));

#if UNPACKAGED
			var expectedSuffix = "Fonts\\dokdo_regular.ttf#Dokdo";
			var root = Path.GetFullPath(Path.Combine(FileSystem.AppDataDirectory, ".."));
			var expected = Path.Combine(root, expectedSuffix);
			expected = Path.GetFullPath(expected);

			var filename = expected[..expected.IndexOf("#", StringComparison.OrdinalIgnoreCase)];
			Assert.True(File.Exists(filename), $"File not found: {filename}");
#else
			var expected = "ms-appdata:///local/Fonts/dokdo_regular.ttf#Dokdo";
#endif

			Assert.Equal(expected, actual.Source);
		});
}

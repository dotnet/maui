using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

public partial class FontManagerTests : TestBase
{
	[Theory]
	[InlineData("dokdo_regular")]
	[InlineData("dokdo_regular.ttf")]
	[InlineData("dokdo_regular#dokdo")]
	[InlineData("dokdo_regular.ttf#dokdo")]
	[InlineData("myalias")]
	public Task CanLoadFonts(string fontName) =>
		InvokeOnMainThreadAsync(() =>
		{
			var registrar = new FontRegistrar(fontLoader: null);
			registrar.Register("dokdo_regular.ttf", "myalias");
			var manager = new FontManager(registrar);

			var actual = manager.GetFontFamily(Font.OfSize(fontName, 12, FontWeight.Regular));

			// Both packaged and unpackaged scenarios should work using the same URI
			// since the WinUI framework knows the difference and correctly maps the
			// URIs to the correct actual font file.
			var expected = "ms-appx:///dokdo_regular.ttf#Dokdo";

			Assert.Equal(expected, actual.Source);
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

			// Packaged and unpackaged scenarios do NOT work with the same URI because we are
			// storing the temporary file in a different location for unpackaged apps.

#if UNPACKAGED
			var expectedSuffix = "Fonts\\dokdo_regular.ttf#Dokdo";
			var root = Path.GetFullPath(Path.Combine(FileSystem.CacheDirectory, ".."));
			var fullFontName = Path.Combine(root, expectedSuffix);
			fullFontName = Path.GetFullPath(fullFontName);

			var filename = fullFontName[..fullFontName.IndexOf("#", StringComparison.OrdinalIgnoreCase)];
			Assert.True(File.Exists(filename), $"File not found: {filename}");
			
			var expected = "ms-appx:///" + fullFontName.Replace("\\", "/", StringComparison.OrdinalIgnoreCase);
#else
			var expected = "ms-appdata:///temp/Fonts/dokdo_regular.ttf#Dokdo";
#endif

			Assert.Equal(expected, actual.Source);
		});
}

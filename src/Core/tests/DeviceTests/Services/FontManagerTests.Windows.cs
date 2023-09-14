using System;
using System.Threading.Tasks;
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

	// TODO: this is not going to work in unpackaged
	[Fact(
#if WINDOWS
			Skip = "Not working for unpackaged"
#endif
	)]
	public Task CanLoadEmbeddedFont() =>
		InvokeOnMainThreadAsync(() =>
		{
			var registrar = new FontRegistrar(new EmbeddedFontLoader());
			registrar.Register("dokdo_regular.ttf", "myalias", GetType().Assembly);
			var manager = new FontManager(registrar);

			var actual = manager.GetFontFamily(Font.OfSize("myalias", 12, FontWeight.Regular));

			Assert.Equal("ms-appdata:///local/fonts/dokdo_regular.ttf#Dokdo", actual.Source);
		});
}

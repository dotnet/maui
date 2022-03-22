using Android.App;
using Android.Graphics;
using Java.Interop;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.Fonts)]
public partial class FontManagerTests : TestBase
{
	[Theory]
	[InlineData("dokdo_regular", "dokdo_regular.ttf")]
	[InlineData("dokdo_regular.ttf", "dokdo_regular.ttf")]
	[InlineData("dokdo_regular#dokdo", "dokdo_regular.ttf")]
	[InlineData("dokdo_regular.ttf#dokdo", "dokdo_regular.ttf")]
	[InlineData("myalias", "dokdo_regular.ttf")]
	[InlineData("insubfolder", "Fonts/insubfolder.ttf")]
	[InlineData("insubfolder.ttf", "Fonts/insubfolder.ttf")]
	[InlineData("insubfolder#insubfolder", "Fonts/insubfolder.ttf")]
	[InlineData("insubfolder.ttf#insubfolder", "Fonts/insubfolder.ttf")]
	public void CanLoadFonts(string fontName, string assetName)
	{
		var fontWeight = FontWeight.Regular;
		var registrar = new FontRegistrar(fontLoader: null);
		registrar.Register("dokdo_regular.ttf", "myalias");
		var manager = new FontManager(registrar);
		var actual = manager.GetTypeface(Font.OfSize(fontName, 12, fontWeight));
		var expected = Typeface.Create(Typeface.CreateFromAsset(Application.Context.Assets, assetName), (int)fontWeight, italic: false);
		Assert.True(expected.Equals(actual));
	}

	[Fact]
	public void CanLoadEmbeddedFont()
	{
		var fontName = "FooBarFont";
		var fontWeight = FontWeight.Regular;
		var registrar = new FontRegistrar(new EmbeddedFontLoader ());
		registrar.Register("dokdo_regular.ttf", fontName, GetType().Assembly);
		var manager = new FontManager(registrar);
		var actual = manager.GetTypeface(Font.OfSize(fontName, 12, fontWeight));
		var defaultFont = Typeface.Create(Typeface.Create(fontName, TypefaceStyle.Normal), (int)fontWeight, italic: false);
		Assert.False(defaultFont.Equals(actual));
	}
}

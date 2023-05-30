using System;
using Android.App;
using Android.Graphics;
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
		// skip on older androids for now
		// https://github.com/dotnet/maui/issues/5903
		if (!OperatingSystem.IsAndroidVersionAtLeast(28))
			return;

		var fontWeight = FontWeight.Regular;
		var fontStyle = TypefaceStyle.Normal;
		var fontSlant = false;

		Typeface expected;
		var expectedTypeface = Typeface.CreateFromAsset(Application.Context.Assets, assetName);
		if (OperatingSystem.IsAndroidVersionAtLeast(28))
			expected = Typeface.Create(expectedTypeface, (int)fontWeight, fontSlant);
		else
			expected = Typeface.Create(expectedTypeface, fontStyle);

		Assert.NotEqual(Typeface.Default, expected);

		var registrar = new FontRegistrar(fontLoader: null);
		registrar.Register("dokdo_regular.ttf", "myalias");
		var manager = new FontManager(registrar);
		var actual = manager.GetTypeface(Font.OfSize(fontName, 12, fontWeight));

		Assert.NotEqual(Typeface.Default, actual);

		Assert.True(expected.Equals(actual));
	}

	[Fact]
	public void CanLoadEmbeddedFont()
	{
		// skip on older androids for now
		// https://github.com/dotnet/maui/issues/5903
		if (!OperatingSystem.IsAndroidVersionAtLeast(28))
			return;

		var fontName = "FooBarFont";
		var fontWeight = FontWeight.Regular;
		var fontStyle = TypefaceStyle.Normal;
		var fontSlant = false;

		var registrar = new FontRegistrar(new EmbeddedFontLoader());
		registrar.Register("dokdo_regular.ttf", fontName, GetType().Assembly);
		var manager = new FontManager(registrar);
		var actual = manager.GetTypeface(Font.OfSize(fontName, 12, fontWeight));

		Assert.NotEqual(Typeface.Default, actual);

		Typeface expected;
		var expectedTypeface = Typeface.Create(fontName, TypefaceStyle.Normal);
		if (OperatingSystem.IsAndroidVersionAtLeast(28))
			expected = Typeface.Create(expectedTypeface, (int)fontWeight, italic: fontSlant);
		else
			expected = Typeface.Create(expectedTypeface, fontStyle);

		Assert.NotEqual(Typeface.Default, expected);

		Assert.False(expected.Equals(actual));
	}

	[Theory]
	[InlineData("monospace")]
	[InlineData("sansserif")]
	[InlineData("serif")]
	public void CanLoadSystemFonts(string fontName)
	{
		if (!OperatingSystem.IsAndroidVersionAtLeast(28))
			return;

		var registrar = new FontRegistrar(fontLoader: null);
		var manager = new FontManager(registrar);
		var actual = manager.GetTypeface(Font.OfSize(fontName, 12));

		Assert.NotEqual(Typeface.Default, actual);
	}
}

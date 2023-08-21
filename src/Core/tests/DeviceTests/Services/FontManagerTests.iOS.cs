// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.Fonts)]
public partial class FontManagerTests : TestBase
{
	[Theory]
	[InlineData(".SFUI-Bold", ".AppleSystemUIFont")]
	[InlineData(".SFUI-SemiBold", ".AppleSystemUIFont")]
	[InlineData(".SFUI-Black", ".AppleSystemUIFont")]
	[InlineData(".SFUI-Heavy", ".AppleSystemUIFont")]
	[InlineData(".SFUI-Light", ".AppleSystemUIFont")]
	public async System.Threading.Tasks.Task CanLoadSystemFonts(string fontName, string expectedFamilyName)
	{
		var registrar = new FontRegistrar(fontLoader: null);
		var manager = new FontManager(registrar);

		var font = await InvokeOnMainThreadAsync(() =>
			manager.GetFont(Font.OfSize(fontName, manager.DefaultFontSize)));

		Assert.Equal(expectedFamilyName, font.FamilyName);
	}

}

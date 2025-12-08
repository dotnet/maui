// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh6996 : ContentPage
{
	public Gh6996() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void FontImageSourceColorWithDynamicResource(XamlInflator inflator)
		{
			var layout = new Gh6996(inflator);
			Image image = layout.image;
			var fis = image.Source as FontImageSource;
			Assert.Equal(Colors.Orange, fis.Color);

			layout.Resources["imcolor"] = layout.Resources["notBlue"];
			Assert.Equal(Colors.Lime, fis.Color);
		}
	}
}

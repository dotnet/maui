// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh6996 : ContentPage
{
	public Gh6996() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void FontImageSourceColorWithDynamicResource([Values] XamlInflator inflator)
		{
			var layout = new Gh6996(inflator);
			Image image = layout.image;
			var fis = image.Source as FontImageSource;
			Assert.That(fis.Color, Is.EqualTo(Colors.Orange));

			layout.Resources["imcolor"] = layout.Resources["notBlue"];
			Assert.That(fis.Color, Is.EqualTo(Colors.Lime));
		}
	}
}

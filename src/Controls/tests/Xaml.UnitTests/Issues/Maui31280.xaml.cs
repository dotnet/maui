using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui31280 : ContentPage
{
	public Maui31280()
	{
		InitializeComponent();
	}

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void StyleInheritanceShouldWork(XamlInflator inflator)
		{
			var layout = new Maui31280(inflator);
			Assert.Equal(Colors.Green, layout.LabelBaseStyle.TextColor);
			Assert.Equal(Colors.Red, layout.LabelBaseStyleRed.TextColor);
			Assert.Equal(Colors.Red, layout.LabelWithPadding.TextColor);
		}
	}
}

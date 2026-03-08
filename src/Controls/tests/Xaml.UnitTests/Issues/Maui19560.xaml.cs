using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui19560 : ContentPage
{
	public Maui19560()
	{
		InitializeComponent();
	}

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void StylesAreAppliedToShadow(XamlInflator inflator)
		{
			var page = new Maui19560(inflator);
			Assert.Equal(Colors.Red, ((SolidColorBrush)page.label.Shadow.Brush).Color);
			Assert.Equal(0.5, page.label.Shadow.Opacity);
			Assert.Equal(15, page.label.Shadow.Radius);
			Assert.Equal(10, page.label.Shadow.Offset.X);
			Assert.Equal(10, page.label.Shadow.Offset.Y);
		}
	}
}


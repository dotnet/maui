using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue1199 : ContentPage
{
	public Issue1199() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void AllowCreationOfTypesFromString(XamlInflator inflator)
		{
			var layout = new Issue1199(inflator);
			var res = (Color)layout.Resources["AlmostSilver"];

			Assert.Equal(Color.FromArgb("#FFCCCCCC"), res);
		}
	}
}
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class ImplicitResourceDictionaries : ContentPage
{
	public ImplicitResourceDictionaries() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void ImplicitRDonContentViews(XamlInflator inflator)
		{
			var layout = new ImplicitResourceDictionaries(inflator);
			Assert.Equal(Colors.Purple, layout.label.TextColor);
		}
	}
}

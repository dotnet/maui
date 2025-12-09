using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh4572 : ContentPage
{
	public Gh4572() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void BindingAsElement(XamlInflator inflator)
		{
			var layout = new Gh4572(inflator) { BindingContext = new { labeltext = "Foo" } };
			Assert.Equal("Foo", layout.label.Text);
		}
	}
}
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh4572 : ContentPage
{
	public Gh4572() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void BindingAsElement(XamlInflator inflator)
		{
			var layout = new Gh4572(inflator) { BindingContext = new { labeltext = "Foo" } };
			Assert.Equal("Foo", layout.label.Text);
		}
	}
}
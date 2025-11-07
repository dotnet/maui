using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh6648 : ContentPage
{
	public Gh6648() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void BindingsOnxNullDataTypeWorks(XamlInflator inflator)
		{
			var layout = new Gh6648(inflator);
			layout.stack.BindingContext = new { foo = "Foo" };
			Assert.Equal("Foo", layout.label.Text);
		}
	}
}

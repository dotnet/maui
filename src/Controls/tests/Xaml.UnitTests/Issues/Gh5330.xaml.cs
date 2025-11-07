using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh5330 : ContentPage
{
	public Gh5330() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void DoesntFailOnxType(XamlInflator inflator)
		{
			new Gh5330(inflator);
		}

		[Theory]
		[Values]
		public void CompiledBindingWithxType(XamlInflator inflator)
		{
			var layout = new Gh5330(inflator) { BindingContext = new Button { Text = "Foo" } };
			Assert.Equal("Foo", layout.label.Text);
		}
	}
}

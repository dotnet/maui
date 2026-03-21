using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh4102VM
{
	public Gh4102VM SomeNullValue { get; set; }
	public string SomeProperty { get; set; } = "Foo";
}

public partial class Gh4102 : ContentPage
{
	public Gh4102() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void CompiledBindingsNullInPath(XamlInflator inflator)
		{
			var layout = new Gh4102(inflator) { BindingContext = new Gh4102VM() };
			Assert.Null(layout.label.Text);
		}
	}
}

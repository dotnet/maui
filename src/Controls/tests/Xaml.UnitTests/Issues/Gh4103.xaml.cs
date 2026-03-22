using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh4103VM
{
	public string SomeNullableValue { get; set; } = "initial";
}

public partial class Gh4103 : ContentPage
{
	public Gh4103() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void CompiledBindingsTargetNullValue(XamlInflator inflator)
		{
			var layout = new Gh4103(inflator) { BindingContext = new Gh4103VM() };
			Assert.Equal("initial", layout.label.Text);

			layout.BindingContext = new Gh4103VM { SomeNullableValue = null };
			Assert.Equal("target null", layout.label.Text);
		}
	}
}
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh4103VM
{
	public string SomeNullableValue { get; set; } = "initial";
}

public partial class Gh4103 : ContentPage
{
	public Gh4103() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void CompiledBindingsTargetNullValue(XamlInflator inflator)
		{
			var layout = new Gh4103(inflator) { BindingContext = new Gh4103VM() };
			Assert.Equal("initial", layout.label.Text);

			layout.BindingContext = new Gh4103VM { SomeNullableValue = null };
			Assert.Equal("target null", layout.label.Text);
		}
	}
}
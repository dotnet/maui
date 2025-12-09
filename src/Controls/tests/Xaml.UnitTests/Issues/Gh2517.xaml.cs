using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// related to https://github.com/dotnet/maui/issues/23711
public partial class Gh2517 : ContentPage
{
	public Gh2517() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void BindingWithInvalidPathIsNotCompiled(XamlInflator inflator)
		{
			var view = new Gh2517(inflator);

			var binding = view.Label.GetContext(Label.TextProperty).Bindings.GetValue();
			Assert.IsType<Binding>(binding);
		}
	}
}

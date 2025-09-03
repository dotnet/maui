using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// related to https://github.com/dotnet/maui/issues/23711
public partial class Gh2517 : ContentPage
{
	public Gh2517() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void BindingWithInvalidPathIsNotCompiled([Values] XamlInflator inflator)
		{
			var view = new Gh2517(inflator);

			var binding = view.Label.GetContext(Label.TextProperty).Bindings.GetValue();
			Assert.That(binding, Is.TypeOf<Binding>());
		}
	}
}

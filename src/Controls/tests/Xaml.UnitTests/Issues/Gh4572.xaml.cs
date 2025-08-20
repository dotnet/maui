using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh4572 : ContentPage
{
	public Gh4572() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void BindingAsElement([Values] XamlInflator inflator)
		{
			var layout = new Gh4572(inflator) { BindingContext = new { labeltext = "Foo" } };
			Assert.That(layout.label.Text, Is.EqualTo("Foo"));
		}
	}
}
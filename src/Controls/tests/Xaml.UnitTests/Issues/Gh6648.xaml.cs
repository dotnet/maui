using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh6648 : ContentPage
{
	public Gh6648() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void BindingsOnxNullDataTypeWorks([Values] XamlInflator inflator)
		{
			var layout = new Gh6648(inflator);
			layout.stack.BindingContext = new { foo = "Foo" };
			Assert.That(layout.label.Text, Is.EqualTo("Foo"));
		}
	}
}

using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh4102VM
{
	public Gh4102VM SomeNullValue { get; set; }
	public string SomeProperty { get; set; } = "Foo";
}

public partial class Gh4102 : ContentPage
{
	public Gh4102() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void CompiledBindingsNullInPath([Values] XamlInflator inflator)
		{
			var layout = new Gh4102(inflator) { BindingContext = new Gh4102VM() };
			Assert.That(layout.label.Text, Is.EqualTo(null));
		}
	}
}

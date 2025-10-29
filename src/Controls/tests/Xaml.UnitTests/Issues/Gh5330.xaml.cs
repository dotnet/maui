using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh5330 : ContentPage
{
	public Gh5330() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void DoesntFailOnxType([Values] XamlInflator inflator)
		{
			new Gh5330(inflator);
		}

		[Test]
		public void CompiledBindingWithxType([Values] XamlInflator inflator)
		{
			var layout = new Gh5330(inflator) { BindingContext = new Button { Text = "Foo" } };
			Assert.That(layout.label.Text, Is.EqualTo("Foo"));
		}
	}
}

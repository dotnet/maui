using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh4103VM
{
	public string SomeNullableValue { get; set; } = "initial";
}

public partial class Gh4103 : ContentPage
{
	public Gh4103() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void CompiledBindingsTargetNullValue([Values] XamlInflator inflator)
		{
			var layout = new Gh4103(inflator) { BindingContext = new Gh4103VM() };
			Assert.That(layout.label.Text, Is.EqualTo("initial"));

			layout.BindingContext = new Gh4103VM { SomeNullableValue = null };
			Assert.That(layout.label.Text, Is.EqualTo("target null"));
		}
	}
}
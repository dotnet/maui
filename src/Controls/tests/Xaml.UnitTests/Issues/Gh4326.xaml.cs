using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh4326 : ContentPage
{
	public static string Foo = "Foo";
	internal static string Bar = "Bar";

	public Gh4326() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void FindStaticInternal([Values] XamlInflator inflator)
		{
			var layout = new Gh4326(inflator);

			Assert.That(layout.labelfoo.Text, Is.EqualTo("Foo"));
			Assert.That(layout.labelbar.Text, Is.EqualTo("Bar"));
			Assert.That(layout.labelinternalvisibleto.Text, Is.EqualTo(Style.StyleClassPrefix));
		}
	}
}

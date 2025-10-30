using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh3280 : ContentPage
{
	public Gh3280() => InitializeComponent();

	public Size Foo { get; set; }

	[TestFixture]
	class Tests
	{
		[Test]
		public void SizeHasConverter([Values] XamlInflator inflator)
		{
			Gh3280 layout = null;
			Assert.DoesNotThrow(() => layout = new Gh3280(inflator));
			Assert.That(layout.Foo, Is.EqualTo(new Size(15, 25)));
		}
	}
}

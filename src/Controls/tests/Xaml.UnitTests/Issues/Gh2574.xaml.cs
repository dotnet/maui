using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh2574 : ContentPage
{
	public Gh2574() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void xNameOnRoot([Values] XamlInflator inflator)
		{
			var layout = new Gh2574(inflator);
			Assert.That(layout.page, Is.EqualTo(layout));
		}
	}
}

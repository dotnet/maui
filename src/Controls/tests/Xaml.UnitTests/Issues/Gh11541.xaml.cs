using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh11541 : ContentPage
{
	public Gh11541() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void RectangleGeometryDoesntThrow([Values] XamlInflator inflator) => Assert.DoesNotThrow(() => new Gh11541(inflator));
	}
}

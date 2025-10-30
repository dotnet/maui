using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.A;

public partial class Bz31234 : ContentPage
{
	public Bz31234() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void ShouldPass([Values] XamlInflator inflator)
		{
			new Bz31234(inflator);
			Assert.Pass();
		}
	}
}
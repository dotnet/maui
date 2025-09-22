using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui2304Closed
{
	public Maui2304Closed() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void XamlCDoesntFail([Values] XamlInflator inflator)
		{
			var layout = new Maui2304Closed(inflator);
			Assert.AreEqual(typeof(OnPlatform<string>), typeof(Maui2304Closed).BaseType);
		}
	}
}

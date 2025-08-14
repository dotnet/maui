using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh6361 : ContentPage
{
	public Gh6361() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void CSSBorderRadiusDoesNotFail([Values] XamlInflator inflator)
		{
			var layout = new Gh6361(inflator);
		}
	}
}

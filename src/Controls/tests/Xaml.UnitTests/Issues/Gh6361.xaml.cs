using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh6361 : ContentPage
{
	public Gh6361() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void CSSBorderRadiusDoesNotFail(XamlInflator inflator)
		{
			var layout = new Gh6361(inflator);
		}
	}
}

using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh2574 : ContentPage
{
	public Gh2574() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void xNameOnRoot(XamlInflator inflator)
		{
			var layout = new Gh2574(inflator);
			Assert.Equal(layout, layout.page);
		}
	}
}

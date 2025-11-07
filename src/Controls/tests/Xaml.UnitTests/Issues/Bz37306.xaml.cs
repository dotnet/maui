using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz37306 : ContentPage
{
	public Bz37306() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void xStringInResourcesDictionaries(XamlInflator inflator)
		{
			var layout = new Bz37306(inflator);
			Assert.Equal("Mobile App", layout.Resources["AppName"]);
			Assert.Equal("Mobile App", layout.Resources["ApplicationName"]);
		}
	}
}

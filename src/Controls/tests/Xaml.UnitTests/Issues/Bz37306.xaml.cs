using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz37306 : ContentPage
{
	public Bz37306() => InitializeComponent();


	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void xStringInResourcesDictionaries(XamlInflator inflator)
		{
			var layout = new Bz37306(inflator);
			Assert.Equal("Mobile App", layout.Resources["AppName"]);
			Assert.Equal("Mobile App", layout.Resources["ApplicationName"]);
		}
	}
}

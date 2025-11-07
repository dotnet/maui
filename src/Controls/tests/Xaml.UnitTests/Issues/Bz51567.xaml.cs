using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz51567 : ContentPage
{
	public Bz51567()
	{
		InitializeComponent();
	}


	public class Tests
	{
		[Theory]
		[Values]
		public void SetterWithElementValue(XamlInflator inflator)
		{
			var page = new Bz51567(inflator);
			var style = page.Resources["ListText"] as Style;
			var setter = style.Setters[1];
			Assert.NotNull(setter);
		}
	}
}

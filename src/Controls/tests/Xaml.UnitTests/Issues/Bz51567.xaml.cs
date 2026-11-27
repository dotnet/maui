using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz51567 : ContentPage
{
	public Bz51567()
	{
		InitializeComponent();
	}

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void SetterWithElementValue(XamlInflator inflator)
		{
			var page = new Bz51567(inflator);
			var style = page.Resources["ListText"] as Style;
			var setter = style.Setters[1];
			Assert.NotNull(setter);
		}
	}
}

using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz60788 : ContentPage
{
	public Bz60788() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{

		[Theory]
		[XamlInflatorData]
		internal void KeyedRDWithImplicitStyles(XamlInflator inflator)
		{
			var layout = new Bz60788(inflator);
			Assert.Equal(2, layout.Resources.Count);
			Assert.Equal(3, ((ResourceDictionary)layout.Resources["RedTextBlueBackground"]).Count);
			Assert.Equal(3, ((ResourceDictionary)layout.Resources["BlueTextRedBackground"]).Count);
		}
	}
}
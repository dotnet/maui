using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz51567 : ContentPage
{
	public Bz51567()
	{
		InitializeComponent();
	}

	[TestFixture]
	class Tests
	{
		[Test]
		public void SetterWithElementValue([Values] XamlInflator inflator)
		{
			var page = new Bz51567(inflator);
			var style = page.Resources["ListText"] as Style;
			var setter = style.Setters[1];
			Assert.NotNull(setter);
		}
	}
}

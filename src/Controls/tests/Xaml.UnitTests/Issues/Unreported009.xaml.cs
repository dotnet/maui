using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Unreported009 : ContentPage
{
	public Unreported009() => InitializeComponent();

	public class Tests
	{
		[Theory]
		[Values]
		public void AllowSetterValueAsElementProperties(XamlInflator inflator)
		{
			var p = new Unreported009(inflator);
			var s = p.Resources["Default"] as Style;
			Assert.Equal("Bananas!", (s.Setters[0].Value as Label).Text);
		}
	}
}

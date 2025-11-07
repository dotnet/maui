using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue1438 : ContentPage
{
	public Issue1438() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void XNameForwardDeclaration(XamlInflator inflator)
		{
			var page = new Issue1438(inflator);

			var slider = page.FindByName<Slider>("slider");
			var label = page.FindByName<Label>("label");
			Assert.Same(slider, label.BindingContext);
			Assert.IsType<StackLayout>(slider.Parent);
		}
	}
}
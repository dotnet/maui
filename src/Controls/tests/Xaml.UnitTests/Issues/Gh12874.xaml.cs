using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh12874 : ContentPage
{
	public Gh12874() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void RevertToStyleValue(XamlInflator inflator)
		{
			var layout = new Gh12874(inflator);
			Assert.Equal(LayoutOptions.Start, layout.label0.HorizontalOptions);
			Assert.Equal(LayoutOptions.Start, layout.label1.HorizontalOptions);
			layout.label0.ClearValue(Label.HorizontalOptionsProperty);
			layout.label1.ClearValue(Label.HorizontalOptionsProperty);
			Assert.Equal(LayoutOptions.Center, layout.label0.HorizontalOptions);
			Assert.Equal(LayoutOptions.Center, layout.label1.HorizontalOptions);
		}
	}
}

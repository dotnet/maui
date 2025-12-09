using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class LabelHtml : ContentPage
{
	public LabelHtml() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void HtmlInCDATA(XamlInflator inflator)
		{
			var html = "<h1>Hello World!</h1><br/>SecondLine";
			var layout = new LabelHtml(inflator);
			Assert.Equal(html, layout.label0.Text);
			Assert.Equal(html, layout.label1.Text);
			Assert.Equal(html, layout.label2.Text);
			Assert.Equal(html, layout.label3.Text);
			Assert.Equal(html, layout.label4.Text);
		}
	}
}
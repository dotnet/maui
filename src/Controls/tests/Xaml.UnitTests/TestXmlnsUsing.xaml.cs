using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class TestXmlnsUsing : ContentPage
{
	public TestXmlnsUsing() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void SupportUsingXmlns(XamlInflator inflator)
		{
			var page = new TestXmlnsUsing(inflator);
			Assert.NotNull(page.Content);
			Assert.IsType<CustomXamlView>(page.CustomView);
			Assert.Equal(1, page.Radio1.Value);
			Assert.Equal(2, page.Radio2.Value);
		}
	}
}

using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XNull : ContentPage
{
	public XNull() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void SupportsXNull(XamlInflator inflator)
		{
			var layout = new XNull(inflator);
			Assert.True(layout.Resources.ContainsKey("null"));
			Assert.Null(layout.Resources["null"]);
		}
	}
}
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XNull : ContentPage
{
	public XNull() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void SupportsXNull(XamlInflator inflator)
		{
			var layout = new XNull(inflator);
			Assert.True(layout.Resources.ContainsKey("null"));
			Assert.Null(layout.Resources["null"]);
		}
	}
}
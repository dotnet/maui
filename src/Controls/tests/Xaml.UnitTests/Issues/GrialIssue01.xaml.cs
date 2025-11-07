using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class GrialIssue01 : ContentPage
{
	public GrialIssue01() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void ImplicitCastIsUsedOnFileImageSource(XamlInflator inflator)
		{
			var layout = new GrialIssue01(inflator);
			var res = (FileImageSource)layout.Resources["image"];

			Assert.Equal("path.png", res.File);
		}
	}
}
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class GrialIssue01 : ContentPage
{
	public GrialIssue01() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void ImplicitCastIsUsedOnFileImageSource([Values] XamlInflator inflator)
		{
			var layout = new GrialIssue01(inflator);
			var res = (FileImageSource)layout.Resources["image"];

			Assert.AreEqual("path.png", res.File);
		}
	}
}
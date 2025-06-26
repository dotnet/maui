using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21314 : _IssuesUITest
	{
		public override string Issue => "Image has wrong orientation on iOS";

		public Issue21314(TestDevice device) : base(device)
		{
		}

		[Fact]
		[Trait("Category", UITestCategories.Image)]
		public void ImageShouldBePortrait()
		{
			var image = App.WaitForElement("theImage").GetRect();
			Assert.Greater(image.Height, image.Width);
		}
	}
}
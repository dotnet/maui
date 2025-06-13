using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class FontImageUITest : _IssuesUITest
	{
		public override string Issue => "FontImage UI Test";

		public FontImageUITest(TestDevice device)
		: base(device)
		{ }

		[Fact]
		[Category(UITestCategories.Image)]
		public void VerifyFontImage()
		{
			App.WaitForElement("Image");
			VerifyScreenshot();
		}
	}
}

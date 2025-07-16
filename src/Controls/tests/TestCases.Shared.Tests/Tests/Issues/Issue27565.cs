using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27565 : _IssuesUITest
	{
		public Issue27565(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Buttons with Gradient and ImageSource at the same time: ImageSource doesn't appear";

		[Test]
		[Category(UITestCategories.Button)]

		public void ImageInButtonShouldBeVisibleWithGradientBackground()
		{
			App.WaitForElement("Button");
			VerifyScreenshot();
		}
	}
}
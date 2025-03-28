using NUnit.Framework;
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

		[Test]
		[Category(UITestCategories.Image)]
		public void VerifyFontImage()
		{
			App.WaitForElement("Image");
			VerifyScreenshot();
		}
	}
}

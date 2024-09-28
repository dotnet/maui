using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class BorderAndShadowDrawingVerifications : _IssuesUITest
	{
		public BorderAndShadowDrawingVerifications(TestDevice device) : base(device) { }

		public override string Issue => "Border and shadow drawing verifications";

		[Test]
		[Category(UITestCategories.Border)]
		public async Task BorderAndShadowShouldRenderAndChangeProperly()
		{
			// Initial screenshot
			App.WaitForElement("ChangeSize");
			VerifyScreenshot("BorderShadowInitialSize");

			// Change size of the border, animation should not lag, this can only be verified manually
			App.Click("ChangeSize");
			await Task.Delay(250);
			VerifyScreenshot("BorderShadowResized");

			// Change shadow radius
			App.Click("ChangeShadow");
			VerifyScreenshot("BorderShadowRadiusChanged");

			// Display a border for the first time (initially IsVisible=false)
			App.Click("ShowHide");
			VerifyScreenshot("BorderShadowShownForTheFirstTime");

			// Change shape of the border
			App.Click("ChangeShape");
			VerifyScreenshot("BorderShadowShapeChanged");

			// Change clip of the border
			App.Click("ChangeClip");
			VerifyScreenshot("BorderShadowClipChanged");
		}
	}
}
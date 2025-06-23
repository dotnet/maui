#if TEST_FAILS_ON_ANDROID // This test fails randomly, likely because the image source has disappeared sometimes in CI, not able to reproduce locally. For now, we have ignored the test for Android. Issue for re-enabling: https://github.com/dotnet/maui/issues/24243
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14471 : _IssuesUITest
{
	public Issue14471(TestDevice device) : base(device) { }

	public override string Issue => "Image can disappear when going back to the page";

	[Test]
	[Category(UITestCategories.Image)]
	public void ImageDoesntDisappearWhenNavigatingBack()
	{
		App.WaitForElement("image");
		App.Click("switchToTab2Button");
		App.WaitForElement("switchToTab1Button");
		App.Click("switchToTab1Button");
		App.WaitForElement("image");

		// The test passes if image is loaded
		VerifyScreenshot();
	}
}
#endif
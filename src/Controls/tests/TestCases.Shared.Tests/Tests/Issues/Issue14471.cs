#if !MACCATALYST // VerifyScreenshot() is not supported on MacCatalyst
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
	[FailsOnAndroidWhenRunningOnXamarinUITest("Suddenly failing. https://github.com/dotnet/maui/issues/24243")]
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
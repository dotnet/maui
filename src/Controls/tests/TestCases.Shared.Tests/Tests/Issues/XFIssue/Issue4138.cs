#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS // This test is specific to iOS because: Only on iOS is the title icon centered; on other platforms, it's aligned to the left.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue4138 : _IssuesUITest
{
	public Issue4138(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] NavigationPage.TitleIcon no longer centered";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void TitleIconIsCentered()
	{
		App.WaitForElement("ContentPage");
		VerifyScreenshot();
	}
}
#endif
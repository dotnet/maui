#if ANDROID
#if TEST_FAILS_ON_ANDROID // SetNavigationBarColor and SetStatusBarColor are deprecated APIs in Android 13 and above. We have enabled edge-to-edge by default, so these will not work after enabling edge-to-edge mode.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28552 : _IssuesUITest
{
	public Issue28552(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Changing StatusBar and NavigationBar background color doesn't work with Modal pages";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void StatusBarAndNavigationBarShouldInheritColor()
	{
		App.WaitForElement("OpenModalButton");
		App.Click("OpenModalButton");
		App.WaitForElement("LabelOnModalPage");
		VerifyScreenshot();
	}
}
#endif
#endif
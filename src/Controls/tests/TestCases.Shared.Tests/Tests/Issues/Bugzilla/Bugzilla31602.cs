#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST // Setting orientation is not supported on Windows and Mac
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla31602 : _IssuesUITest
{
	public Bugzilla31602(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "not possible to programmatically open master page after iPad landscape -> portrait rotation, also tests 31664";

	[Test]

	[Category(UITestCategories.FlyoutPage)]
	public void Bugzilla31602Test()
	{
		App.WaitForElement("Sidemenu Opener");
		App.Tap("Sidemenu Opener");
		App.WaitForElement("SideMenu");
		App.SetOrientationLandscape();
		OpenFlyout();
		App.SetOrientationPortrait();
		OpenFlyout();

	}

	void OpenFlyout()
	{
		// Condition to ensure consistent behavior across platforms, for the flyout remains open on Android but closes on iOS during device orientation changes.
#if IOS
		App.WaitForElement("Sidemenu Opener");
		App.Tap("Sidemenu Opener");
#endif
		App.WaitForElement("SideMenu");
	}

	[TearDown]
	public void TearDown()
	{
		App.SetOrientationPortrait();
	}
}
#endif
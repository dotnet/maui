#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS //The test fails on Windows and MacCatalyst because the SetOrientation method, which is intended to change the device orientation, is only supported on mobile platforms iOS and Android.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue15154 : _IssuesUITest
{
	public Issue15154(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "iOS Flyout title is not broken over multiple lines when you rotate your screen";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShouldFlyoutTextWrapsInLandscape()
	{
		App.WaitForElement("OpenFlyoutButton");
		App.Tap("OpenFlyoutButton");
		App.SetOrientationLandscape();
#if ANDROID
		VerifyScreenshot(cropLeft: 125);
#else
		VerifyScreenshot();
#endif
	}
}
#endif
#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS //The test fails on Windows and MacCatalyst because the SetOrientation method, which is intended to change the device orientation, is only supported on mobile platforms Android and iOS.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8870 : _IssuesUITest
{
	const string Success = "Success";
	const string CheckResult = "Check";

	public Issue8870(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] CollectionView with HTML Labels Freeze the Screen on Rotation";

	[Test]
	[Category(UITestCategories.CollectionView)]
	[FailsOnMacWhenRunningOnXamarinUITest("SetOrientationPortrait method not implemented")]
	[FailsOnWindowsWhenRunningOnXamarinUITest("SetOrientationPortrait method not implemented")]
	public void RotatingCollectionViewWithHTMLShouldNotHangOrCrash()
	{
		App.WaitForElement(CheckResult);

		App.SetOrientationPortrait();
		App.WaitForElement(CheckResult);

		App.SetOrientationLandscape();
		App.WaitForElement(CheckResult);

		App.SetOrientationPortrait();
		App.WaitForElement(CheckResult);

		App.SetOrientationLandscape();
		App.WaitForElement(CheckResult);

		App.SetOrientationPortrait();
		App.WaitForElement(CheckResult);

		App.TapWithRetriesUntilElementReady(CheckResult, Success, maxRetries: 10);

		App.WaitForElement(Success);
	}
}
#endif
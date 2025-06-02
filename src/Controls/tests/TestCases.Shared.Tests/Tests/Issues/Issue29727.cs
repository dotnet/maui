#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS //The test fails on Windows and MacCatalyst because the SetOrientation method, which is intended to change the device orientation, is only supported on mobile platforms iOS and Android.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29727 : _IssuesUITest
{
	public Issue29727(TestDevice device) : base(device) { }

	public override string Issue => "I1 - Vertical list for Item Height- After rotating the Android emulator, some text boxes have extra blank space";

	[Test]
	[Category(UITestCategories.Label)]
	public void LabelShouldSizeProperlyOnCollectionViewWithoutBlankSpace()
	{
		App.WaitForElement("TestCollectionView");
		App.Tap("ScrollToDownButton");
		App.SetOrientationLandscape();
		App.WaitForElement("TestCollectionView");
		VerifyScreenshot();
	}
}
#endif
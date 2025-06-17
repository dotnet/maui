#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS //The test fails on Windows and MacCatalyst because the SetOrientation method, which is intended to change the device orientation, is only supported on mobile platforms iOS and Android.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29542 : _IssuesUITest
{
	public Issue29542(TestDevice device) : base(device) { }

	public override string Issue => "I1_Vertical_list_for_Multiple_Rows - Rotating the emulator would cause clipping on the description text.";

	[Test]
	[Category(UITestCategories.Label)]
	public void LabelShouldSizeProperlyOnCollectionView()
	{
		App.WaitForElement("TestCollectionView");
		App.Tap("ScrollToDownButton");
		App.SetOrientationLandscape();
		App.WaitForElement("TestCollectionView");
		VerifyScreenshot();
	}

	[TearDown]
	public void TearDown()
	{
		App.SetOrientationPortrait();
	}
}
#endif
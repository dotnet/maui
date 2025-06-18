#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST
// This test is skipped on iOS and Catalyst due to potential flakiness.
// In the sample uses a delay which can cause the test to fail randomly.
// To maintain test reliability, we exclude this test on these platforms.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue11523 : _IssuesUITest
{
	public Issue11523(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] FlyoutBehavior.Disabled removes back-button from navbar";

	[Test]
	[Category(UITestCategories.Shell)]
	public void BackButtonStillVisibleWhenFlyoutBehaviorDisabled()
	{
		App.WaitForElement("PageLoaded");
		App.TapBackArrow();
		App.WaitForFlyoutIcon(FlyoutIconAutomationId);
	}
}
#endif
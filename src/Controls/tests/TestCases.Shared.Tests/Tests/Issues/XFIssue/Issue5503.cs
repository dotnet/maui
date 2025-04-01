#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST // This test is only applicable for iOS as here tested the native property behavior.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue5503 : _IssuesUITest
{
	const string ChangeBackgroundButtonAutomationId = "ChangeBackgroundButton";
	const string ListViewAutomationId = "TheListView";
	const string GoToTestPage = "Go To Test Page";

	public Issue5503(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] UITableView.Appearance.BackgroundColor ignored or overridden for ListView";

	[Test]
	[Category(UITestCategories.ListView)]
	public void ToggleAppearanceApiBackgroundColorListView()
	{
		App.WaitForElement(GoToTestPage);
		App.Tap(GoToTestPage);

		App.WaitForElement(ChangeBackgroundButtonAutomationId);
		App.Tap(ChangeBackgroundButtonAutomationId);

		App.TapBackArrow();

		App.WaitForElement(GoToTestPage);
		App.Tap(GoToTestPage);

		App.WaitForElement(ChangeBackgroundButtonAutomationId);

		VerifyScreenshot();
	}
}
#endif
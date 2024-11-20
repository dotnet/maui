using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue5503 : _IssuesUITest
{
	public Issue5503(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] UITableView.Appearance.BackgroundColor ignored or overridden for ListView";

	// TODO: in the old tests this was marked as manual review, can we still automate this somehow?
	//[Test]
	//[Category(UITestCategories.ListView)]
	//[Category(UITestCategories.ManualReview)]
	//[FailsOnIOS]
	//public void ToggleAppearanceApiBackgroundColorListView()
	//{
	//	App.WaitForElement(ChangeBackgroundButtonAutomationId);

	//	App.Screenshot("ListView cells have clear background, default color from code");

	//	App.Tap(ChangeBackgroundButtonAutomationId);
	//	App.NavigateBack();
	//	App.WaitForNoElement(ChangeBackgroundButtonAutomationId);
	//	AppSetup.NavigateToIssue(typeof(Issue5503), RunningApp);
	//	App.WaitForElement(ChangeBackgroundButtonAutomationId);

	//	App.Screenshot("ListView cells have Red background, set by Appearance API");

	//}
}
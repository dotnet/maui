using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3008 : _IssuesUITest
{
	const string ClickUntilSuccess = "ClickUntilSuccess";

	const string NotGroupedItem = "NotGroupedItem";

	const string GroupedItem = "GroupedItem";

	public Issue3008(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Setting ListView.ItemSource to null doesn't cause it clear out its contents";

	[Test]
	[Category(UITestCategories.ListView)]
	public void EnsureListViewEmptiesOut()
	{
		App.WaitForElement(ClickUntilSuccess);
		App.Tap(ClickUntilSuccess);
		App.WaitForElement(NotGroupedItem);
		App.WaitForElement(GroupedItem);

		App.Tap(ClickUntilSuccess);
		App.WaitForElement(NotGroupedItem);
		App.WaitForElement(GroupedItem);

		App.Tap(ClickUntilSuccess);
		App.WaitForNoElement(NotGroupedItem);
		App.WaitForNoElement(GroupedItem);

		App.Tap(ClickUntilSuccess);
		App.WaitForElement(NotGroupedItem);
		App.WaitForElement(GroupedItem);

		App.Tap(ClickUntilSuccess);
		App.WaitForNoElement(NotGroupedItem);
		App.WaitForNoElement(GroupedItem);

		App.Tap(ClickUntilSuccess);
		App.WaitForElement(NotGroupedItem);
		App.WaitForElement(GroupedItem);

		App.Tap(ClickUntilSuccess);
		App.WaitForNoElement(NotGroupedItem);
		App.WaitForNoElement(GroupedItem);
	}
}
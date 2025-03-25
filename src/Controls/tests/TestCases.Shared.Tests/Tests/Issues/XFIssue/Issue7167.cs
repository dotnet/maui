#if TEST_FAILS_ON_CATALYST //ScrollDown is not working on Catalyst
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7167 : _IssuesUITest
{
	public Issue7167(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] improved observablecollection. a lot of collectionchanges. a reset is sent and listview scrolls to the top";


	const string ListViewId = "ListViewId";
	const string AddRangeCommandId = "AddRangeCommandId";

	[Test]
	[Category(UITestCategories.ListView)]
	public void Issue7167Test()
	{
		// add items to the list and scroll down till item "23"
		App.WaitForElement(AddRangeCommandId);
		App.Tap(AddRangeCommandId);
		App.Tap(AddRangeCommandId);

		// No equivalent method found in Appium. Also this method is not necessary to validate the test case.
		// App.Print.Tree();

		App.ScrollDown(ListViewId, ScrollStrategy.Auto, 0.65, 200);
		App.WaitForAnyElement(["15", "20", "30", "40", "60", "80"]);

		// when adding additional items via a addrange and a CollectionChangedEventArgs.Action.Reset is sent
		// then the listview shouldnt reset or it should not scroll to the top
		App.Tap(AddRangeCommandId);

		App.WaitForAnyElement(["15", "20", "30", "40", "60", "80"]);
	}
}
#endif
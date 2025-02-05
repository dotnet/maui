#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // Swipe, ScrollDown not working on Catalyst, In iOS, WaitForNoElement throws a timeout exception eventhough the text is not visible on the UI. 
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class ShellGestures : _IssuesUITest
{
	public ShellGestures(TestDevice testDevice) : base(testDevice)
	{
	}
	const string SwipeTitle = "Swipe";
	const string SwipeGestureSuccess = "SwipeGesture Success";
	const string SwipeGestureSuccessId = "SwipeGestureSuccessId";
	const string TableViewTitle = "Table View";
	const string TableViewId = "TableViewId";
	const string ListViewTitle = "List View";
	const string ListViewId = "ListViewId";


	public override string Issue => "Shell Gestures Test";
	[Test]
	[Category(UITestCategories.Gestures)]
	public void SwipeGesture()
	{
		App.TapInShellFlyout(SwipeTitle);
		App.WaitForElement(SwipeGestureSuccessId);
		App.SwipeLeftToRight(SwipeGestureSuccessId);
		Assert.That(App.WaitForElement(SwipeGestureSuccessId).GetText(), Is.EqualTo(SwipeGestureSuccess));
	}

	[Test]
	[Category(UITestCategories.TableView)]
	public void TableViewScroll()
	{
		App.TapInShellFlyout(TableViewTitle);
		App.WaitForElement(TableViewId);
		App.ScrollDown(TableViewId, ScrollStrategy.Gesture, 0.30, 400);

		// Verifying that first item in TableView is not visible also confirms that the TableView has scrolled.
		App.WaitForNoElement("section1");
	}

	[Test]
	[Category(UITestCategories.ListView)]
	public void ListViewScroll()
	{
		App.TapInShellFlyout(ListViewTitle);
		App.WaitForElement(ListViewId);
		App.ScrollDown(ListViewId, ScrollStrategy.Gesture, 0.20, 200);

		// Verifying that first item in ListView is not visible also confirms that the ListView has scrolled.
		App.WaitForNoElement("0 Entry");
	}
}
#endif
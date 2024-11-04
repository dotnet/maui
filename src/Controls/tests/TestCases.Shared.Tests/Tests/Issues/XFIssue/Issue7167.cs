#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS
using System.Drawing;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7167 : _IssuesUITest
{
	const string ListViewId = "ListViewId";
	const string AddRangeCommandId = "AddRangeCommandId";

	public Issue7167(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] improved observablecollection. a lot of collectionchanges. a reset is sent and listview scrolls to the top";

	[Test]
	[Category(UITestCategories.ListView)]
	public void Issue7167Test()
	{
		// Arrange
		// Add items to the list and scroll down till item "25"
		App.Screenshot("Empty ListView");
		App.Tap(AddRangeCommandId);
		App.Tap(AddRangeCommandId);
		App.PrintTree();
		App.ScrollDownTo("25", ListViewId, ScrollStrategy.Gesture);
		App.WaitForElement("25");

		// Act
		// When adding additional items via a addrange and a CollectionChangedEventArgs.Action.Reset is sent
		// Then the listview shouldnt reset or it should not scroll to the top
		App.Tap(AddRangeCommandId);

		// Assert
		// Assert that item "25" is still visible
		App.WaitForElement("25");
		var result = App.FindElementByText("25").GetRect();
		ClassicAssert.AreNotEqual(result, Rectangle.Empty);
	}
}
#endif
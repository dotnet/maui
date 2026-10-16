using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38330 : _IssuesUITest
{
	public override string Issue => "CollectionView cells overlap after swiping a SwipeView item";

	public Issue38330(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.SwipeView)]
	public void GroupedItemsDoNotOverlapAfterSwipeCloses()
	{
		App.WaitForElement("Row A 2");

		var swipeTarget = App.WaitForElement("Row B 00").GetRect();
		App.WaitForElement("Row B 01");

		App.DragCoordinates(
			swipeTarget.X + swipeTarget.Width - 10,
			swipeTarget.CenterY(),
			swipeTarget.X + 10,
			swipeTarget.CenterY());

		VerifyScreenshot();
	}
}

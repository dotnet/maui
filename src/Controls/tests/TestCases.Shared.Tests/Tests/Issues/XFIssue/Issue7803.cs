using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7803 : _IssuesUITest
{
	public Issue7803(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] CarouselView/RefreshView pull to refresh command firing twice on a single pull";

	//[Test]
	//[Category(UITestCategories.CollectionView)]
	//[FailsOnMauiIOS]
	//public void DelayedIsRefreshingAndCommandTest_SwipeDown()
	//{
	//	var collectionView = App.WaitForElement(q => q.Marked("CollectionView7803"))[0];

	//	App.Pan(new Drag(collectionView.Rect, Drag.Direction.TopToBottom, Drag.DragLength.Medium));

	//	App.WaitForElement(q => q.Marked("Count: 20"));
	//	App.WaitForNoElement(q => q.Marked("Count: 30"));

	//	AppResult[] lastCellResults = null;

	//	App.QueryUntilPresent(() =>
	//	{
	//		App.DragCoordinates(collectionView.Rect.CenterX, collectionView.Rect.Y + collectionView.Rect.Height - 50, collectionView.Rect.CenterX, collectionView.Rect.Y + 5);

	//		lastCellResults = App.Query("19");

	//		return lastCellResults;
	//	}, 10, 1);

	//	Assert.IsTrue(lastCellResults?.Any() ?? false);
	//}
}
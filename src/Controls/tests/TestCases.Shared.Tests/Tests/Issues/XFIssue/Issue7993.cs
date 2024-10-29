using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7993 : _IssuesUITest
{
	public Issue7993(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] CollectionView.Scrolled event offset isn't correctly reset when items change";

	//[Test]
	//[Category(UITestCategories.CollectionView)]
	//public void CollectionViewVerticalOffset()
	//{
	//	var colView = App.WaitForElement("CollectionView7993")[0];

	//	App.WaitForElement(x => x.Marked("VerticalOffset: 0"));

	//	AppResult[] lastCellResults = null;

	//	App.QueryUntilPresent(() =>
	//	{
	//		App.DragCoordinates(colView.Rect.CenterX, colView.Rect.Y + colView.Rect.Height - 50, colView.Rect.CenterX, colView.Rect.Y + 5);

	//		lastCellResults = App.Query("19");

	//		return lastCellResults;
	//	}, 20, 1);

	//	Assert.IsTrue(lastCellResults?.Any() ?? false);

	//	App.Tap(x => x.Marked("NewItemsSource"));
	//	App.WaitForElement(x => x.Marked("VerticalOffset: 0"));
	//}
}
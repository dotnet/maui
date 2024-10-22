using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1414 : _IssuesUITest
{
	public Issue1414(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "InvalidCastException when scrolling and refreshing TableView";

	// [Test]
	// [Category(UITestCategories.TableView)]
	// public void InvalidCastExceptionWhenScrollingAndRefreshingTableView()
	// {
	// 	RunningApp.Screenshot("Start G1414");
	// 	var tableFrame = RunningApp.WaitForElement(q => q.Marked("TableView"))[0].Rect;
	// 	RunningApp.ScrollForElement("* marked:'Row-4-24'", new Drag(tableFrame, Drag.Direction.BottomToTop, Drag.DragLength.Long));
	// 	RunningApp.Screenshot("Scrolled to end without crashing!");
	// 	RunningApp.ScrollForElement("* marked:'Row-0-0'", new Drag(tableFrame, Drag.Direction.TopToBottom, Drag.DragLength.Long));
	// 	RunningApp.Screenshot("Scrolled to top without crashing!");
	// }
}
#if TEST_FAILS_ON_CATALYST //ScrollDown and ScrollUp are not working on MacCatalyst
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

	[Test]
	[Category(UITestCategories.TableView)]
	public void InvalidCastExceptionWhenScrollingAndRefreshingTableView()
	{
		App.WaitForElement("TableView");
		App.ScrollDown("TableView", ScrollStrategy.Gesture, 1.0, 50);
		App.WaitForAnyElement(["Cell 4-24", "Cell 0-24"]);
		App.ScrollUp("TableView", ScrollStrategy.Gesture, 1.0, 30);
		App.WaitForAnyElement(["Cell 0-0", "Cell 3-24"]);
	}
}
#endif
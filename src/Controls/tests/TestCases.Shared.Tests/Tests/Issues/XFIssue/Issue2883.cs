using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.TableView)]
public class Issue2883 : _IssuesUITest
{
	public Issue2883(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ViewCell IsEnabled set to false does not disable a cell in a TableView";

	// [UITests.FailsOnIOS]
	// [Test]
	// public void Issue2883TestDisabled ()
	// {
	// 	App.Screenshot ("I am at Issue 2883");
	// 	App.Tap( c=> c.Marked("btnCustomCellTable"));
	// 	App.WaitForNoElement( c=> c.Marked("Clicked"));
	// 	App.Screenshot ("I dont see the disable cell");
	// 	App.Tap( c=> c.Marked("btnCustomCellListView"));
	// 	App.WaitForNoElement( c=> c.Marked("Clicked"));
	// 	App.Screenshot ("I dont see the disable cell");
	// }

	// [UITests.FailsOnIOS]
	// [Test]
	// public void Issue2883TestEnabled ()
	// {

	// 	App.Tap( c=> c.Marked("btnCustomCellTableEnabled"));
	// 	App.Screenshot ("I see the cell that is enabled");
	// 	App.WaitForElement( c=> c.Marked("Clicked"));
	// 	App.Tap (c => c.Marked ("ok"));
	// 	App.Tap( c=> c.Marked("btnCustomCellListViewEnabled"));
	// 	App.WaitForElement( c=> c.Marked("Clicked"));
	// 	App.Tap (c => c.Marked ("ok"));
	// }
}
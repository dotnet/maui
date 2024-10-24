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
	// 	RunningApp.Screenshot ("I am at Issue 2883");
	// 	RunningApp.Tap( c=> c.Marked("btnCustomCellTable"));
	// 	RunningApp.WaitForNoElement( c=> c.Marked("Clicked"));
	// 	RunningApp.Screenshot ("I dont see the disable cell");
	// 	RunningApp.Tap( c=> c.Marked("btnCustomCellListView"));
	// 	RunningApp.WaitForNoElement( c=> c.Marked("Clicked"));
	// 	RunningApp.Screenshot ("I dont see the disable cell");
	// }

	// [UITests.FailsOnIOS]
	// [Test]
	// public void Issue2883TestEnabled ()
	// {

	// 	RunningApp.Tap( c=> c.Marked("btnCustomCellTableEnabled"));
	// 	RunningApp.Screenshot ("I see the cell that is enabled");
	// 	RunningApp.WaitForElement( c=> c.Marked("Clicked"));
	// 	RunningApp.Tap (c => c.Marked ("ok"));
	// 	RunningApp.Tap( c=> c.Marked("btnCustomCellListViewEnabled"));
	// 	RunningApp.WaitForElement( c=> c.Marked("Clicked"));
	// 	RunningApp.Tap (c => c.Marked ("ok"));
	// }
}
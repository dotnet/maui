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

	[Test]
	public void Issue2883TestDisabled()
	{
		App.WaitForElement("btnCustomCellTable");
		App.Tap("btnCustomCellTable");
		App.WaitForNoElement("Clicked");

		App.Tap("btnCustomCellListView");
		App.WaitForNoElement("Clicked");
	}


	[Test]
	public void Issue2883TestEnabled()
	{
		App.WaitForElement("btnCustomCellTableEnabled");
		App.Tap("btnCustomCellTableEnabled");
		App.WaitForElement("Clicked");
		App.TapDisplayAlertButton("ok");
		App.WaitForElement("btnCustomCellListViewEnabled");
		App.Tap("btnCustomCellListViewEnabled");
		App.WaitForElement("Clicked");
		App.TapDisplayAlertButton("ok");
	}
}
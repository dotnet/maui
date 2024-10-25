using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2289 : _IssuesUITest
{
	public Issue2289(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TextCell IsEnabled property not disabling element in TableView";

	//[Test]
	//[Category(UITestCategories.Cells)]
	//[Ignore("Fails sometimes on XTC")]
	//public void TestIsEnabledFalse()
	//{
	//	var disable1 = RunningApp.Query(c => c.Marked("txtCellDisable1"))[0];
	//	Assert.IsFalse(disable1.Enabled);
	//	var disable2 = RunningApp.Query(c => c.Marked("txtCellDisable2"))[0];
	//	Assert.IsFalse(disable2.Enabled);
	//}

	//[Test]
	//[Ignore("Fails sometimes on XTC")]
	//public void TestIsEnabledFalseContextActions()
	//{
	//	var disable1 = RunningApp.Query(c => c.Marked("txtCellDisableContextActions1"))[0];
	//	Assert.IsFalse(disable1.Enabled);

	//	var screenBounds = RunningApp.RootViewRect();

	//	RunningApp.DragCoordinates(screenBounds.Width - 10, disable1.Rect.CenterY, 10, disable1.Rect.CenterY);

	//	RunningApp.Screenshot("Not showing context menu");
	//	RunningApp.WaitForNoElement(c => c.Marked("More"));
	//	RunningApp.TapCoordinates(screenBounds.CenterX, screenBounds.CenterY);
	//}

	//[Test]
	//[Ignore("Fails sometimes on XTC")]
	//public void TestIsEnabledTrue()
	//{
	//	var disable1 = RunningApp.Query(c => c.Marked("txtCellEnable1"))[0];
	//	Assert.IsTrue(disable1.Enabled);
	//	var disable2 = RunningApp.Query(c => c.Marked("txtCellEnable2"))[0];
	//	Assert.IsTrue(disable2.Enabled);
	//}

	//[Test]
	//[Ignore("Fails sometimes on XTC")]
	//public void TestIsEnabledTrueContextActions()
	//{
	//	var disable1 = RunningApp.Query(c => c.Marked("txtCellEnabledContextActions1"))[0];
	//	Assert.IsTrue(disable1.Enabled);

	//	var screenBounds = RunningApp.RootViewRect();

	//	RunningApp.DragCoordinates(screenBounds.Width - 10, disable1.Rect.CenterY, 10, disable1.Rect.CenterY);

	//	RunningApp.Screenshot("Showing context menu");
	//	RunningApp.WaitForElement(c => c.Marked("More"));
	//}
}
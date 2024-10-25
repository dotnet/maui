using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Shell)]
public class ShellTitleView : _IssuesUITest
{
	public ShellTitleView(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Title View Test";

	//	[Test]
	//public void TitleWidthMeasuresCorrectly_13949()
	//{
	//	this.TapInFlyout("Width Measure (13949)");
	//	RunningApp.WaitForElement("Text");
	//	RunningApp.WaitForElement("B1");
	//	RunningApp.WaitForElement("B2");
	//}

	//[Test]
	//public void TitleWidthWithToolBarItemMeasuresCorrectly_13949()
	//{
	//	this.TapInFlyout("Width Measure and ToolBarItem (13949)");
	//	RunningApp.WaitForElement("Text");
	//	RunningApp.WaitForElement("B1");
	//	RunningApp.WaitForElement("B2");
	//}

	//[Test]
	//public void TitleViewPositionsCorrectly()
	//{
	//	var titleView = RunningApp.WaitForElement("TitleViewId")[0].Rect;
	//	var topTab = RunningApp.WaitForElement("page 2")[0].Rect;

	//	var titleViewBottom = titleView.Y + titleView.Height;
	//	var topTabTop = topTab.Y;

	//	Assert.GreaterOrEqual(topTabTop, titleViewBottom, "Title View is incorrectly positioned behind tabs");
	//}

	//[Test]
	//public void NoDuplicateTitleViews()
	//{
	//	var titleView = RunningApp.WaitForElement("TitleViewId");

	//	Assert.AreEqual(1, titleView.Length);

	//	RunningApp.Tap("page 2");
	//	RunningApp.Tap("page 3");
	//	RunningApp.Tap("page 4");
	//	titleView = RunningApp.WaitForElement("TitleViewId");

	//	Assert.AreEqual(1, titleView.Length);
	//}
}
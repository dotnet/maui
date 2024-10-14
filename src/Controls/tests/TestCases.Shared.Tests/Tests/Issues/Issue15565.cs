using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.TitleView)]
public class Issue15565 : _IssuesUITest
{
	public Issue15565(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] Shell TitleView and ToolBarItems rendering strange display on iOS 16";

	// [Test]
	// public void TitleViewHeightIsNotZero()
	// {
	// 	var titleView = RunningApp.WaitForElement("TitleViewId")[0].Rect;
	// 	var topTab = RunningApp.WaitForElement("page 1")[0].Rect;

	// 	var titleViewBottom = titleView.Y + titleView.Height;
	// 	var topTabTop = topTab.Y;

	// 	Assert.GreaterOrEqual(topTabTop, titleViewBottom, "Title View is incorrectly positioned in iOS 16");
	// }

	// [FailsOnAndroid]
	// [FailsOnIOS]
	// [Test]
	// public void ToolbarItemsWithTitleViewAreRendering()
	// {
	// 	RunningApp.WaitForElement("Item 1");
	// 	RunningApp.WaitForElement("Item 3");
	// }

	// [Test]
	// public void NoDuplicateTitleViews()
	// {
	// 	var titleView = RunningApp.WaitForElement("TitleViewId");

	// 	Assert.AreEqual(1, titleView.Length);

	// 	RunningApp.Tap("page 1");
	// 	RunningApp.Tap("page 2");
	// 	RunningApp.Tap("page 3");

	// 	titleView = RunningApp.WaitForElement("TitleViewId");

	// 	Assert.AreEqual(1, titleView.Length);
	// }
}
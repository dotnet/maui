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
	// 	var titleView = App.WaitForElement("TitleViewId")[0].Rect;
	// 	var topTab = App.WaitForElement("page 1")[0].Rect;

	// 	var titleViewBottom = titleView.Y + titleView.Height;
	// 	var topTabTop = topTab.Y;

	// 	Assert.GreaterOrEqual(topTabTop, titleViewBottom, "Title View is incorrectly positioned in iOS 16");
	// }

	// [FailsOnAndroid]
	// [FailsOnIOS]
	// [Test]
	// public void ToolbarItemsWithTitleViewAreRendering()
	// {
	// 	App.WaitForElement("Item 1");
	// 	App.WaitForElement("Item 3");
	// }

	// [Test]
	// public void NoDuplicateTitleViews()
	// {
	// 	var titleView = App.WaitForElement("TitleViewId");

	// 	Assert.AreEqual(1, titleView.Length);

	// 	App.Tap("page 1");
	// 	App.Tap("page 2");
	// 	App.Tap("page 3");

	// 	titleView = App.WaitForElement("TitleViewId");

	// 	Assert.AreEqual(1, titleView.Length);
	// }
}
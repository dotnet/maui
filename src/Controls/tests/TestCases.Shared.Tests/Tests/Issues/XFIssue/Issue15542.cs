using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue15542 : _IssuesUITest
{
	public Issue15542(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] Shell.TitleView does not render on iOS 16";

	// [Test]
	// [Category(UITestCategories.TitleView)]
	// public void TitleViewHeightDoesntOverflow()
	// {
	// 	var titleView = App.WaitForElement("TitleViewId")[0].Rect;
	// 	var topTab = App.WaitForElement("page 1")[0].Rect;

	// 	var titleViewBottom = titleView.Y + titleView.Height;
	// 	var topTabTop = topTab.Y;

	// 	Assert.GreaterOrEqual(topTabTop, titleViewBottom, "Title View is incorrectly positioned in iOS 16");
	// }
}
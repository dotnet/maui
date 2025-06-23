using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue15542 : _IssuesUITest
{
	const string Page1 = "page 1";
	public Issue15542(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] Shell.TitleView does not render on iOS 16";

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleViewHeightDoesntOverflow()
	{
		var titleView = App.WaitForElement("title 1").GetRect();
#if WINDOWS
		App.Tap("navViewItem");
#endif
		App.WaitForTabElement(Page1);
		var topTab = App.WaitForTabElement(Page1).GetRect();

		var titleViewBottom = titleView.Y + titleView.Height;
		var topTabTop = topTab.Y;

		Assert.That(topTabTop, Is.GreaterThanOrEqualTo(titleViewBottom), "Title View is incorrectly positioned in iOS 16");
	}
}

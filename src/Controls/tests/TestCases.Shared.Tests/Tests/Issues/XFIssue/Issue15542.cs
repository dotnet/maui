using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue15542 : _IssuesUITest
{

#if ANDROID
	const string Page1 = "PAGE 1";
#else
	const string Page1 = "page 1";
#endif

	public Issue15542(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] Shell.TitleView does not render on iOS 16";

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleViewHeightDoesntOverflow()
	{
		var titleView = App.WaitForElement("title 1").GetRect();
#if WINDOWS // In Windows the Page 1 items are inside the root navViewItem which in popup we neet top it once.
		App.Tap("navViewItem");
#endif
		var topTab = App.WaitForElement(Page1).GetRect();

		var titleViewBottom = titleView.Y + titleView.Height;
		var topTabTop = topTab.Y;

		Assert.That(topTabTop, Is.GreaterThanOrEqualTo(titleViewBottom), "Title View is incorrectly positioned in iOS 16");
	}
}

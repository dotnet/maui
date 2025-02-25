using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue892 : _IssuesUITest
{
#if ANDROID
	const string OnePushed = "";
	const string InitialPage = "";
	const string Page5 = "";
#else
	const string OnePushed = "One pushed";
	const string InitialPage = "Initial Page";
	const string Page5 = "Page 5";
#endif
	public Issue892(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "NavigationPages as details in FlyoutPage don't work as expected";


	[Test]
	[Category(UITestCategories.FlyoutPage)]
	[Description("Change pages in the Flyout ListView, and navigate to the end and back")]
	public void Issue892TestsNavigateChangePagesNavigate()
	{
		NavigateToEndAndBack(InitialPage);
		App.Tap("Present Flyout");
		App.Tap(Page5);

#if ANDROID || WINDOWS // IsPresented value not reflected when changing on ItemTapped in FlyoutPage More Information: https://github.com/dotnet/maui/issues/26324.
		App.WaitForElementTillPageNavigationSettled(Page5);
		App.TapInFlyoutPageFlyout("Close Flyout");
#else
		App.Tap("Close Flyout");
#endif
		NavigateToEndAndBack(Page5);
	}

	void NavigateToEndAndBack(string BackButtonId)
	{
		App.WaitForElement("Push next page");
		App.Tap("Push next page");
		App.WaitForElement("Push next next page");
		App.Tap("Push next next page");
		App.WaitForElement("You are at the end of the line");
		App.Tap("Check back one");
		App.WaitForElement("Pop one");
		App.TapBackArrow(OnePushed);
		App.WaitForElementTillPageNavigationSettled("Check back two");
		App.Tap("Check back two");
		App.WaitForElement("Pop two");
		App.WaitForElementTillPageNavigationSettled("Check back two");
		App.TapBackArrow(BackButtonId);
		App.Tap("Check back three");
		App.WaitForElement("At root");
	}
}
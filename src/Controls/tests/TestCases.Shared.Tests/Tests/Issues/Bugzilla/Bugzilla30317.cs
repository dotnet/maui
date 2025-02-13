using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.TabbedPage)]
public class Bugzilla30317 : _IssuesUITest
{

#if WINDOWS //On Windows title AutomationId is not working, so using text to find the element. 
	const string PageOne = "Set ItemSource On Appearing";
	const string PageTwo = "Set ItemSource in ctor"; 
#else
	const string PageOne = "PageOne";
	const string PageTwo = "PageTwo";
#endif

#if IOS || MACCTALYST
	const string TabOne = "TabbedPageOne";
	const string TabTwo = "TabbedPageTwo";
#else
	const string TabOne = "TabOneCtor";
	const string TabTwo = "TabTwoOnAppearing";
#endif
	const string PageTwoButton = "GoToPageTwoButton";
	const string PageThreeButton = "GoToPageThreeButton";
	const string PageOneItem1 = "PageOneItem1";
	const string PageOneItem5 = "PageOneItem5";
	const string PageTwoItem1 = "PageTwoItem1";
	const string PageTwoItem5 = "PageTwoItem5";

	const string TabOneItem1 = "PageThreeTabOneItem1";
	const string TabOneItem5 = "PageThreeTabOneItem5";
	const string TabTwoItem1 = "PageThreeTabTwoItem1";
	const string TabTwoItem5 = "PageThreeTabTwoItem5";

	public Bugzilla30317(TestDevice testDevice) : base(testDevice)
	{
	}

	protected override bool ResetAfterEachTest => true;

	public override string Issue => "https://bugzilla.xamarin.com/show_bug.cgi?id=30137";

	[Test]
	public void Bugzilla30317ItemSourceOnAppearingContentPage()
	{
		App.WaitForElement(PageOne);

		App.WaitForElement(PageOneItem1);
		TouchAndHold(PageOneItem1);

		App.WaitForElement(PageOneItem5);
		TouchAndHold(PageOneItem5);
	}

	[Test]
	public void Bugzilla30317ItemSourceCtorContentPage()
	{
		App.WaitForElement(PageTwoButton);
		App.Tap(PageTwoButton);
		App.WaitForElementTillPageNavigationSettled(PageTwo);

		App.WaitForElement(PageTwoItem1);
		TouchAndHold(PageTwoItem1);

		App.WaitForElement(PageTwoItem5);
		TouchAndHold(PageTwoItem5);
	}

	[Test]
	public void Bugzilla30317ItemSourceTabbedPage()
	{
		App.WaitForElement(PageTwoButton);
		App.Tap(PageTwoButton);
		App.WaitForElementTillPageNavigationSettled(PageTwo);

		App.WaitForElement(PageThreeButton);
		App.Tap(PageThreeButton);
		App.TapTab(TabTwo);

		App.WaitForElementTillPageNavigationSettled(TabTwoItem1);
		TouchAndHold(TabTwoItem1);
		App.WaitForElement(TabTwoItem1);

		App.WaitForElement(TabTwoItem5);
		TouchAndHold(TabTwoItem5);
		App.WaitForElement(TabTwoItem5);

		App.TapTab(TabOne);

		App.WaitForElement(TabOneItem1);
		TouchAndHold(TabOneItem1);
		App.WaitForElement(TabOneItem1);

		App.WaitForElement(TabOneItem5);
		TouchAndHold(TabOneItem5);
		App.WaitForElement(TabOneItem5);
	}

	void TouchAndHold(string item)
	{
#if MACCATALYST //In Catalyst TouchAndHold is not supported. So using LongPress
      	App.LongPress(item);
#else
		App.TouchAndHold(item);
#endif
	}
}

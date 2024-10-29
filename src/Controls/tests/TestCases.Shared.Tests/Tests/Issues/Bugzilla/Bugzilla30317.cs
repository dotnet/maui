/*
#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.TabbedPage)]
public class Bugzilla30317 : _IssuesUITest
{
	public Bugzilla30317(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "https://bugzilla.xamarin.com/show_bug.cgi?id=30137";

	[Test]
	public void Bugzilla30317ItemSourceOnAppearingContentPage()
	{
		App.Screenshot("I am at Bugzilla30317");
		App.WaitForElement("GoToPageTwoButton");
		App.Screenshot("I see Page 1");

		App.WaitForElement("PageOneItem1");
		App.TouchAndHold("PageOneItem1");

		App.WaitForElement("PageOneItem5");
		App.TouchAndHold("PageOneItem5");

		App.Screenshot("I did not crash");
	}

	[Test]
	public void Bugzilla30317ItemSourceCtorContentPage()
	{
		App.WaitForElement("GoToPageTwoButton");
		App.Tap("GoToPageTwoButton");

		App.WaitForElement("PageTwo");
		App.Screenshot("I see Page 2");

		App.WaitForElement("PageTwoItem1");
		App.TouchAndHold("PageTwoItem1");

		App.WaitForElement("PageTwoItem5");
		App.TouchAndHold("PageTwoItem5");

		App.Screenshot("I did not crash");
	}

	[Test]
	public void Bugzilla30317ItemSourceTabbedPage()
	{
		App.WaitForElement("GoToPageTwoButton");
		App.Tap("GoToPageTwoButton");

		App.Screenshot("I see Page 2");
		App.WaitForElement("PageTwo");

		App.WaitForElement("GoToPageThreeButton");
		App.Tap("GoToPageThreeButton");

		App.Screenshot("I see TabbedPage One");
		App.WaitForElement("TabOneCtor");

		App.WaitForElement("PageThreeTabOneItem1");
		App.TouchAndHold("PageThreeTabOneItem1");
		App.WaitForElement("PageThreeTabOneItem1");

		App.WaitForElement("PageThreeTabOneItem5");
		App.TouchAndHold("PageThreeTabOneItem5");
		App.WaitForElement("PageThreeTabOneItem5");

		App.Screenshot("I see TabbedPage Two");
		App.WaitForElement("TabTwoOnAppearing");
		App.Tap("TabTwoOnAppearing");

		App.WaitForElement("PageThreeTabTwoItem1");
		App.TouchAndHold("PageThreeTabTwoItem1");
		App.WaitForElement("PageThreeTabTwoItem1");

		App.WaitForElement("PageThreeTabTwoItem5");
		App.TouchAndHold("PageThreeTabTwoItem5");
		App.WaitForElement("PageThreeTabTwoItem5");
	}
}
#endif
*/

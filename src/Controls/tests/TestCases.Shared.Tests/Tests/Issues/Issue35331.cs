#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35331 : _IssuesUITest
{
	public Issue35331(TestDevice device) : base(device) { }

	public override string Issue => "Android TabbedPage inside Modal Navigation does not overlay BottomNavigationView after PushAsync";

	[Test]
	[Order(1)]
	[Category(UITestCategories.TabbedPage)]
	public void ModalTabbedPageDetailPageGoBackRestoresTabBar()
	{
		// Open the modal TabbedPage
		App.WaitForElement("OpenModalButton");
		App.Tap("OpenModalButton");

		App.WaitForElement("Tab1Label");

		// Push detail page
		App.Tap("PushDetailButton");
		App.WaitForElement("DetailPageLabel");

		// Go back from detail page
		App.Tap("GoBackButton");

		// Verify we're back on Tab 1 with the tab bar restored
		App.WaitForElement("Tab1Label");

		// Dismiss the modal so the next test starts from a clean state
		App.Back();
		App.WaitForElement("OpenModalButton");
	}

	[Test]
	[Order(2)]
	[Category(UITestCategories.TabbedPage)]
	public void ModalTabbedPagePushAsyncShouldOverlayBottomNavigationView()
	{
		// Open the modal TabbedPage (with bottom tabs on Android)
		App.WaitForElement("OpenModalButton");
		App.Tap("OpenModalButton");

		// Verify Tab 1 is visible
		App.WaitForElement("Tab1Label");

		// Push the detail page from Tab 1 via PushAsync
		App.Tap("PushDetailButton");

		// Wait for the detail page to appear
		App.WaitForElement("DetailPageLabel");

		// The detail page pushed via PushAsync should fully overlay the screen,
		// hiding the BottomNavigationView (tab bar). Verify visually.
		VerifyScreenshot();
	}

}
#endif

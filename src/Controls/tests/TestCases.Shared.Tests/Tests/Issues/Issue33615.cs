using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33615 : _IssuesUITest
{
	public Issue33615(TestDevice device) : base(device) { }

	public override string Issue => "[Android] Title of FlyOutPage is not updating anymore after showing a NonFlyOutPage";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void TitleUpdatesAfterShowingNonFlyoutPage()
	{
		// Wait for the FlyoutPage detail (DetailPage1) to load
		App.WaitForElement("ShowNonFlyoutButton");

		// Show a NonFlyoutPage by changing Window.Page
		App.Tap("ShowNonFlyoutButton");

		// Wait for the NonFlyoutPage to appear
		App.WaitForElement("BackToFlyoutButton");

		// Go back to the FlyoutPage
		App.Tap("BackToFlyoutButton");

		// Wait for the FlyoutPage to be restored with DetailPage1
		App.WaitForElement("NavigateToDetailPage2Button");

		// Navigate to DetailPage2 — the toolbar title should update to "DetailPage2"
		App.Tap("NavigateToDetailPage2Button");

		// Verify DetailPage2 is the current page
		App.WaitForElement("DetailPage2Label");

		// The toolbar title must match "DetailPage2" — use VerifyScreenshot to
		// detect if the toolbar is frozen on the old title "DetailPage1"
		VerifyScreenshot();
	}
}

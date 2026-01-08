using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19747 : _IssuesUITest
{
	public Issue19747(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell BackButtonBehavior TextOverride text gets truncated";

	[Test]
	[Category(UITestCategories.Shell)]
	public void BackButtonTextOverrideShouldNotBeTruncated()
	{
		// Wait for the initial page to load and navigate to the page with TextOverride
		App.WaitForElementTillPageNavigationSettled("Navigate to Page 2");
		App.Tap("Navigate to Page 2");

		// Wait for Page 2 to load where the back button with TextOverride="Cancel" is visible
		App.WaitForElement("StatusLabel");

		// Verify the back button text is not truncated via screenshot comparison
		// With the fix, the text should shrink to fit. Without the fix, text is truncated.
		VerifyScreenshot();
	}
}

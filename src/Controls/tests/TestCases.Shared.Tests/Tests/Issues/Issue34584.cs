#if ANDROID // This test validates Android-specific layout behavior with soft keyboard and navigation
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34584 : _IssuesUITest
{
	public Issue34584(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Content renders under status bar when navigating with keyboard open to a page with NavBarIsVisible=False";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ContentShouldNotRenderUnderStatusBarAfterNavigatingWithKeyboardOpen()
	{
		// Wait for the main page to load
		App.WaitForElement("Entry");

		// Tap the Entry to open the soft keyboard
		App.Tap("Entry");

		// Enter text to ensure the IME is fully visible
		App.EnterText("Entry", "test");

		// Tap the navigate button to go to the destination page
		App.Tap("NavigateButton");

		// Wait for the destination page to load
		App.WaitForElement("TargetLabel");

		// Get the rect of the label on the destination page
		var labelRect = App.FindElement("TargetLabel").GetRect();

		// The label should be positioned below the status bar (Y > 0)
		// If Y == 0, the content is rendering under the status bar which is the bug
		Assert.That(labelRect.Y, Is.GreaterThan(0),
			"TargetLabel should not be at Y=0 (would be under the status bar)");
	}
}
#endif

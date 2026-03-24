#if ANDROID
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

		// Focus Entry and open keyboard
		App.Tap("Entry");

		// Enter text to ensure IME is active
		App.EnterText("Entry", "test");

		//  Trigger navigation while IME is still open
		App.PressEnter();

		// Wait for destination page
		App.WaitForElement("TargetLabel");

		var label = App.FindElement("TargetLabel");
		Assert.That(label, Is.Not.Null);

		var labelRect = label.GetRect();

		Assert.That(labelRect.Y, Is.GreaterThan(0),
			"TargetLabel should not be at Y=0 (would be under the status bar)");
	}
}
#endif
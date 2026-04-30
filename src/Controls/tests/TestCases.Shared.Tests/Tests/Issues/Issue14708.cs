#if ANDROID || iOS // Orientation changes and IME behavior are only relevant on mobile platforms
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14708 : _IssuesUITest
{
	public Issue14708(TestDevice device) : base(device) { }

	public override string Issue => "Android SearchBar in landscape shows full-screen IME extract mode";

	[TearDown]
	public void TearDown()
	{
		App.SetOrientationPortrait();
	}

	[Test]
	[Category(UITestCategories.SearchBar)]
	public void SearchBarLandscapeShowsInlineKeyboardNotExtractMode()
	{
		// Rotate to landscape — this is the trigger condition for the bug
		App.SetOrientationLandscape();

		App.WaitForElement("SearchBarControl");

		// Tap the primary SearchBar to open the keyboard
		App.Tap("SearchBarControl");

		// Wait a moment for keyboard to appear
		Thread.Sleep(1000);

		// In the unfixed state, Android enters IME extract mode in landscape:
		// a full-screen black overlay replaces the inline keyboard and covers all
		// page content. VerifyScreenshot() catches this because the visual output
		// is dramatically different from the fixed (inline-keyboard) baseline.
		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
	}
}
#endif

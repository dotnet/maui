using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25200 : _IssuesUITest
{
	public Issue25200(TestDevice device) : base(device) { }

	public override string Issue => "Actionsheet maximum size has been hardcoded on windows, creating display issues";

	[Test]
	[Category(UITestCategories.ActionSheet)]
	public void ActionSheetWithSixActionsShouldDisplayProperly()
	{
		App.WaitForElement("ShowActionSheetButton", timeout: TimeSpan.FromSeconds(30));
		App.Tap("ShowActionSheetButton");

		// Verify the ActionSheet title is visible
		App.WaitForElement("Actionsheet is set to hardcoded maxheight and maxwidth");

		// Verify all options are visible
		App.WaitForElement("Option 1/6");
		App.WaitForElement("Option 2/6");
		App.WaitForElement("Option 3/6");
		App.WaitForElement("Option 4/6");
		App.WaitForElement("Option 5/6");
		App.WaitForElement("Option 6/6");

		// Verify Cancel button is visible
		App.WaitForElement("Cancel");

		// Take screenshot to validate proper sizing and dismiss the ActionSheet
		VerifyScreenshotAndDismiss();
	}

	// Skip this test on macOS, after 6 elements the native behavior is to display elements horizontally - https://github.com/dotnet/maui/issues/29085
#if !MACCATALYST
	[Test]
	[Category(UITestCategories.ActionSheet)]
	public void ActionSheetWithManyActionsShouldDisplayProperly()
	{
		App.WaitForElement("ShowLargeActionSheetButton", timeout: TimeSpan.FromSeconds(45));
		App.Tap("ShowLargeActionSheetButton");

		// Verify the ActionSheet title is visible
		App.WaitForElement("This actionsheet has many more options to test if all are visible");

		// Verify some key options are visible
		App.WaitForElement("First Option");
		App.WaitForElement("Sixth Option");

		//Mobile platforms typically limit the number of visible options and require scrolling to see more (there is no need to test)
#if !IOS && !ANDROID
		App.WaitForElement("Twelfth Option");
#endif

		// Verify both Cancel and Confirm buttons are visible
		App.WaitForElement("Cancel");
		App.WaitForElement("Confirm");

		// Take screenshot to validate proper sizing and dismiss the ActionSheet
		VerifyScreenshotAndDismiss();
	}
#endif

	// Skip this test on macOS: the popup displays correctly, but Appium throws an UnknownErrorException when element text is too long.
	// A separate workaround would be required for each element. Aside from the title, this doesn't happen on iOS because the text is clipped.
#if !MACCATALYST
	[Test]
	[Category(UITestCategories.ActionSheet)]
	public void ActionSheetWithLongTitleShouldDisplayProperly()
	{
		App.WaitForElement("ShowLongTitleActionSheetButton");
		App.Tap("ShowLongTitleActionSheetButton");

#if IOS
		// Use XPath for long title to avoid identifier length limitation
		App.WaitForElement(AppiumQuery.ByXPath("//XCUIElementTypeStaticText[@label='This is a very long title that should wrap properly to multiple lines instead of being truncated or causing horizontal overflow issues like it might on Windows']"));
#else
		// Verify the ActionSheet with long title is visible and wraps properly
		App.WaitForElement("This is a very long title that should wrap properly to multiple lines instead of being truncated or causing horizontal overflow issues like it might on Windows");
#endif
		// Verify options are visible including long options
		App.WaitForElement("First Option");
		App.WaitForElement("Second Option - this is a very long option text that should also wrap properly to multiple lines just like it does on Android platform to ensure cross-platform consistency");
		App.WaitForElement("Third Option");
		App.WaitForElement("Fourth Option - another long option to test that multiple long options can all wrap properly without causing display issues or horizontal scrolling like the old Windows implementation");

		// Verify Cancel button is visible
		App.WaitForElement("Cancel");

		// Take screenshot to validate proper sizing and dismiss the ActionSheet
		VerifyScreenshotAndDismiss();
	}
#endif

	// If one test fails, don't block the others with previous ActionSheet still open
	private void VerifyScreenshotAndDismiss()
	{
		try
		{
			VerifyScreenshot();
			App.TapDisplayAlertButton("Cancel", 1);
		}
		catch (Exception)
		{
			App.TapDisplayAlertButton("Cancel", 1);
			throw;
		}
	}
}
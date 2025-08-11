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

		// Take screenshot to validate proper sizing
		VerifyScreenshot();

		// Dismiss the ActionSheet
		App.Tap("Cancel");
	}

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
		App.WaitForElement("Second Option");
		App.WaitForElement("Twelfth Option");
		
		// Verify both Cancel and Confirm buttons are visible
		App.WaitForElement("Cancel");
		App.WaitForElement("Confirm");

		// Take screenshot to validate proper sizing
		VerifyScreenshot();

		// Dismiss the ActionSheet
		App.Tap("Cancel");
	}

	[Test]
	[Category(UITestCategories.ActionSheet)]
	public void ActionSheetWithLongTitleShouldDisplayProperly()
	{
		App.WaitForElement("ShowLongTitleActionSheetButton");
		App.Tap("ShowLongTitleActionSheetButton");

		// Verify the ActionSheet with long title is visible and wraps properly
		App.WaitForElement("This is a very long title that should wrap properly to multiple lines instead of being truncated or causing horizontal overflow issues like it might on Windows");
		
		// Verify options are visible including long options
		App.WaitForElement("First Option");
		App.WaitForElement("Second Option - this is a very long option text that should also wrap properly to multiple lines just like it does on Android platform to ensure cross-platform consistency");
		App.WaitForElement("Third Option");
		App.WaitForElement("Fourth Option - another long option to test that multiple long options can all wrap properly without causing display issues or horizontal scrolling like the old Windows implementation");
		
		// Verify Cancel button is visible
		App.WaitForElement("Cancel");

		// Take screenshot to validate proper text wrapping and sizing
		VerifyScreenshot();

		// Dismiss the ActionSheet
		App.Tap("Cancel");
	}
}
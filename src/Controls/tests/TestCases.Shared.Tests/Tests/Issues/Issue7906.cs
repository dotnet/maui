using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7906 : _IssuesUITest
{
	public override string Issue => "Entry and Editor: option to disable borders and underline (focus)";

	public Issue7906(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Entry)]
	public void UnderlineColorPropertyWorks()
	{
		// Wait for page to load completely
		App.WaitForElement("DefaultEntry");
		
		// Take screenshot showing all underline colors
		// This verifies:
		// 1. Blue underline on BlueUnderlineEntry
		// 2. Hidden underline on TransparentUnderlineEntry
		// 3. Green underline on GreenUnderlineEditor
		// 4. Hidden underline on TransparentUnderlineEditor
		// 5. Blue underline on VSMEntry (unfocused state)
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void UnderlineColorChangesOnFocus()
	{
		// Wait for page to load
		App.WaitForElement("VSMEntry");

		// Take screenshot of VSM entry in unfocused state (should be blue)
		VerifyScreenshot("UnfocusedState");

		// Focus the VSM entry (should change underline to red)
		App.Tap("VSMEntry");

		// Small delay to ensure focus state is applied
		Task.Delay(500).Wait();

		// Take screenshot of VSM entry in focused state (should be red)
		VerifyScreenshot("FocusedState");

		// Unfocus by tapping another element
		App.Tap("DefaultEntry");

		// Small delay to ensure normal state is restored
		Task.Delay(500).Wait();

		// Verify it returns to blue when unfocused
		VerifyScreenshot("ReturnToUnfocused");
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void BackgroundAndUnderlineColorWorkTogether()
	{
		// Wait for the regression test entry to load
		App.WaitForElement("BackgroundAndUnderlineEntry");
		
		// Visual verification that both properties work independently
		// Expected: Red background fill with blue underline
		// NOT expected: Purple/tinted background (would indicate BackgroundTintList regression)
		VerifyScreenshot("BackgroundAndUnderline");
	}
}

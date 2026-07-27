#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28421 : _IssuesUITest
{
	public override string Issue => "ToolbarItems Tooltip wrong theme";

	public Issue28421(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.ToolbarItem)]
	public void ToolbarItemTooltipUsesNativeThemeColors()
	{
		// This test verifies that the tooltip text color uses native Android theme colors
		// instead of inheriting the BarTextColor.
		//
		// Before the fix: tooltip text was red (same as BarTextColor)
		// After the fix: tooltip text uses native Android theme colors (white on dark background)
		App.WaitForElement("InstructionLabel");

		// Get the instruction label to determine screen dimensions
		var labelRect = App.WaitForElement("InstructionLabel").GetRect();
		
		// The toolbar item is in the top-right area of the action bar
		// Calculate position: far right of the screen, near the top
		float x = labelRect.X + labelRect.Width - 50;
		float y = 80;
		
		// Long-press to trigger the tooltip
		App.TouchAndHoldCoordinates(x, y);

		// Verify the tooltip appears with correct native theme colors
		VerifyScreenshot();
	}
}
#endif

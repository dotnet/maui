using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34188 : _IssuesUITest
{
	public Issue34188(TestDevice device) : base(device) { }

	public override string Issue => "Background color doesn't apply correctly to the spanned region on all the platforms.";

	[Test]
	[Category(UITestCategories.Layout)]
	public void SpanningCellBackgroundShouldCoverFullSpannedArea()
	{
		App.WaitForElement("InstructionsLabel");

		// The spanning cell (green) at row=0, col=1 is arranged with height=200,
		// spanning over both row 0 and row 1. Its green background should be visible
		// throughout the entire 200px height area, not just the top 100px.
		// Without the fix, the parent row's red background is visible in the lower half
		// of the spanning area — this screenshot will not match the correct baseline.
		VerifyScreenshot();
	}
}

#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

// Regression test for https://github.com/dotnet/maui/issues/35410
// iOS landscape rotation (landscape-left) caused ScrollView content (e.g., Editor) to be
// placed at x=0 — under the device notch — because UIKit's Automatic
// ContentInsetAdjustmentBehavior only adds top/bottom to AdjustedContentInset for
// vertical scroll views, leaving SystemAdjustedContentInset.Left = 0 even when
// SafeAreaInsets.Left = 44 (notch). The fix reads SafeAreaInsets directly via GetInset().
public class Issue35410 : _IssuesUITest
{
	public override string Issue => "iOS landscape rotation causes ScrollView content to be obscured by device notch";

	public Issue35410(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollViewEditorNotObscuredByNotchInLandscape()
	{
		// Arrange: page loads in portrait, editor must be present
		App.WaitForElement("TestEditor");

		// Act: rotate to landscape — this triggers the safe area recalculation that was broken
		App.SetOrientationLandscape();

		// Allow layout to settle after orientation change
		System.Threading.Thread.Sleep(1500);

		// Assert: editor must still be visible and not hidden under the notch
		App.WaitForElement("TestEditor");

		var editorRect = App.WaitForElement("TestEditor").GetRect();

		Assert.That(editorRect.Width, Is.GreaterThan(0),
			"Editor must have non-zero width in landscape — it should be visible");
		Assert.That(editorRect.Height, Is.GreaterThan(0),
			"Editor must have non-zero height in landscape");

		// Visual verification: screenshot captures whether the editor is under the notch.
		// On a notched device (e.g., iPhone 14 Pro), a broken fix renders the editor
		// partially or fully behind the left notch area.
		VerifyScreenshot();
	}
}
#endif

#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class _TempM3BottomSheetStyle : _IssuesUITest
{
	public override string Issue => "[Android] Shell/TabbedPage More BottomSheet should use M3 theme-aware colors when Material3 is enabled";

	public _TempM3BottomSheetStyle(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Shell)]
	public void MoreBottomSheetUsesM3ThemedColors()
	{
		App.WaitForElement("M3Tab1Content");

		// Tap the "More" overflow tab (appears when >5 tabs exist)
		App.WaitForElement("More");
		App.Tap("More");

		// Wait for the bottom sheet to display overflow items
		App.WaitForElement("Tab5");

		// Verify the bottom sheet visually matches M3 styling
		VerifyScreenshot();
	}
}
#endif

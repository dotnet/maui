#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35127 : _IssuesUITest
{
	public override string Issue => "[Android] Shell/TabbedPage More BottomSheet uses hard-coded M2 colors when Material3 is enabled";

	public Issue35127(TestDevice device) : base(device) { }

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

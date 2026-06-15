#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35902 : _IssuesUITest
{
	public Issue35902(TestDevice device) : base(device) { }

	public override string Issue => "[iOS] Transparent Shell Navigation Bar Breaks After Keyboard Interaction on Secondary Pages";

	[Test]
	[Category(UITestCategories.Shell)]
	public void TransparentShellNavBarShouldRemainTransparentAfterKeyboardDismiss()
	{
		// Navigate to the second page
		App.WaitForElement("NavigateButton");
		App.Tap("NavigateButton");

		// Wait for the second page to load with the entry
		App.WaitForElement("TestEntry");

		// Tap the entry to show the keyboard
		App.Tap("TestEntry");

		// Wait for the keyboard to appear
		Thread.Sleep(1000);

		// Dismiss the keyboard
		App.DismissKeyboard();

		// Wait for the keyboard to fully close and any layout adjustments to complete
		Thread.Sleep(1000);

		// Verify the Shell navigation bar is still transparent (page background color visible through it).
		// If the bug is present, the nav bar will have a solid opaque background instead of transparent.
		VerifyScreenshot();
	}
}
#endif

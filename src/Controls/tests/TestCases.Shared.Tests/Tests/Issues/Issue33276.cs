#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33276 : _IssuesUITest
{
	public override string Issue => "Padding not restored after SoftInput closes";

	public Issue33276(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifySafeAreaPaddingRestoredAfterKeyboardCloses()
	{
		// Wait for the main container to be visible
		App.WaitForElement("MainContainer");

		// Get the bottom border's initial position before keyboard appears
		var bottomBorderBefore = App.FindElement("BottomBorder").GetRect();
		var initialY = bottomBorderBefore.Y;
		var initialBottom = bottomBorderBefore.Y + bottomBorderBefore.Height;

		// Tap the entry to show the keyboard
		App.Tap("TestEntry");

		// Wait for keyboard to appear and layout to adjust
		Thread.Sleep(1500);

		// Dismiss the keyboard
		App.DismissKeyboard();

		// Wait for keyboard to close and layout to restore
		Thread.Sleep(1500);

		// Get the bottom border's position after keyboard closes
		var bottomBorderAfter = App.FindElement("BottomBorder").GetRect();
		var afterY = bottomBorderAfter.Y;
		var afterBottom = bottomBorderAfter.Y + bottomBorderAfter.Height;

		// The bottom border should return to its original position after keyboard closes
		// Allow for small tolerance (within 5 pixels) due to potential animation timing
		Assert.That(afterY, Is.EqualTo(initialY).Within(5),
			$"Bottom border Y position should be restored after keyboard closes. Initial Y: {initialY}px, After Y: {afterY}px");

		Assert.That(afterBottom, Is.EqualTo(initialBottom).Within(5),
			$"Bottom border bottom edge should be restored after keyboard closes. Initial bottom: {initialBottom}px, After bottom: {afterBottom}px");

		// Verify the bottom border is still accessible and has correct dimensions
		Assert.That(bottomBorderAfter.Height, Is.EqualTo(bottomBorderBefore.Height).Within(5),
			"Bottom border height should remain consistent after keyboard opens and closes");
	}
}
#endif

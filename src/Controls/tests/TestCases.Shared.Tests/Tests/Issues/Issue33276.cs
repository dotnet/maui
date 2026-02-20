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

		// Get the bottom marker's initial position before keyboard appears
		// The bottom marker sits right above the safe area padding
		var bottomMarkerBefore = App.FindElement("BottomMarker").GetRect();
		var initialBottom = bottomMarkerBefore.Y + bottomMarkerBefore.Height;

		// Get the padding status label to see initial padding
		var paddingLabelBefore = App.FindElement("PaddingStatusLabel").GetText();
		System.Console.WriteLine($"[Issue33276] Initial padding label: {paddingLabelBefore}");
		System.Console.WriteLine($"[Issue33276] Initial bottom marker bottom edge: {initialBottom}");

		// Tap the entry to show the keyboard
		App.Tap("TestEntry");

		// Wait for keyboard to appear and layout to adjust
		Thread.Sleep(2000);

		// Dismiss the keyboard by tapping outside or using back
		App.DismissKeyboard();

		// Wait for keyboard to close and layout to restore
		Thread.Sleep(2000);

		// Get the bottom marker's position after keyboard closes
		var bottomMarkerAfter = App.FindElement("BottomMarker").GetRect();
		var afterBottom = bottomMarkerAfter.Y + bottomMarkerAfter.Height;

		// Get the padding status label after keyboard closes
		var paddingLabelAfter = App.FindElement("PaddingStatusLabel").GetText();
		System.Console.WriteLine($"[Issue33276] After keyboard padding label: {paddingLabelAfter}");
		System.Console.WriteLine($"[Issue33276] After keyboard bottom marker bottom edge: {afterBottom}");

		// The bottom marker should return to its original position after keyboard closes
		// If the safe area padding is NOT restored, the bottom marker will be lower (closer to screen edge)
		// Allow for small tolerance (within 5 pixels) due to potential animation timing
		Assert.That(afterBottom, Is.EqualTo(initialBottom).Within(5),
			$"Bottom marker position should be restored after keyboard closes. " +
			$"Initial bottom: {initialBottom}px, After bottom: {afterBottom}px. " +
			$"If after > initial, safe area padding was NOT restored (bug #33276).");
	}
}
#endif

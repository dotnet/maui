#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32041Shell : _IssuesUITest
{
	public override string Issue => "Shell with bottom tabs - Keyboard overlaps when SoftInput.AdjustResize is set";

	public Issue32041Shell(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellContainerResizesWithAdjustResize()
	{
		// Wait for the main container to be visible
		App.WaitForElement("MainContainer");

		// Get the container's initial height before keyboard appears
		var containerBeforeKeyboard = App.FindElement("MainContainer").GetRect();
		var initialHeight = containerBeforeKeyboard.Height;

		// Tap the entry to show the keyboard
		App.Tap("TestEntry");

		// Wait for keyboard to appear and layout to adjust
		Task.Delay(1500).Wait();

		// Get the container's height after keyboard appears
		var containerAfterKeyboard = App.FindElement("MainContainer").GetRect();
		var resizedHeight = containerAfterKeyboard.Height;

		// With AdjustResize, the entire Shell container should shrink (height decreases)
		// This ensures the bottom tab bar moves up and stays above the keyboard
		Assert.That(resizedHeight, Is.LessThan(initialHeight),
			$"Shell container should resize (height decrease) when keyboard appears with AdjustResize. Before: {initialHeight}px, After: {resizedHeight}px");

		// The height reduction should be significant (at least 200px for a typical keyboard)
		var heightReduction = initialHeight - resizedHeight;
		Assert.That(heightReduction, Is.GreaterThan(200),
			$"Container should shrink by at least 200px (typical keyboard height). Actual reduction: {heightReduction}px");

		// Verify the bottom marker is still visible and accessible
		var bottomMarker = App.FindElement("BottomMarker");
		Assert.That(bottomMarker, Is.Not.Null, 
			"Bottom marker should remain visible and accessible after keyboard appears");
	}
}
#endif

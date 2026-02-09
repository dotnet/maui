#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32041TabbedPage : _IssuesUITest
{
	public override string Issue => "Keyboard with TabbedPage bottom tabs - verify tabs remain visible";

	public Issue32041TabbedPage(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void VerifyTabbedPageWithKeyboard()
	{
		// Wait for TabbedPage content to load (reuses Issue32041 page content)
		App.WaitForElement("TopLabel");

		// Get initial positions
		var topLabelBefore = App.WaitForElement("TopLabel").GetRect();
		var bottomMarkerBefore = App.WaitForElement("BottomMarker").GetRect();

		var initialTop = topLabelBefore.Top;
		var initialBottom = bottomMarkerBefore.Bottom;

		// Show keyboard
		App.Tap("TestEntry");
		App.WaitForKeyboardToShow();

		// With AdjustResize, content should resize - bottom marker moves up
		var bottomMarkerAfter = App.WaitForElement("BottomMarker").GetRect();
		var topLabelAfter = App.WaitForElement("TopLabel").GetRect();

		// Top label should stay in place (AdjustResize doesn't pan)
		Assert.That(initialTop, Is.EqualTo(topLabelAfter.Top));

		// Bottom marker should move up (content resized)
		Assert.That(bottomMarkerAfter.Bottom, Is.LessThan(initialBottom),
			$"Bottom marker should move up when keyboard appears. Before: {initialBottom}px, After: {bottomMarkerAfter.Bottom}px");

		// Dismiss keyboard
		App.DismissKeyboard();
		App.WaitForKeyboardToHide();

		// Verify layout restores correctly
		var bottomMarkerFinal = App.WaitForElement("BottomMarker").GetRect();

		Assert.That(bottomMarkerFinal.Bottom, Is.EqualTo(initialBottom));
	}
}
#endif
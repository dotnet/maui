#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32041AdjustPan : _IssuesUITest
{
	public override string Issue => "Verify AdjustPan mode does not apply keyboard insets";

	public Issue32041AdjustPan(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyContainerDoesNotResizeWithAdjustPan()
	{
		// Wait for the main container to be visible
		App.WaitForElement("MainContainerPan");
		var rootContainerRect = App.FindElement("RootPagePan").GetRect();
		// Verify the bottom marker is visible and accessible before keyboard
		App.WaitForElement("BottomMarkerPan");
		var bottomMarkerBefore = App.FindElement("BottomMarkerPan").GetRect();
		var topMarkerBefore = App.FindElement("TopLabelPan").GetRect();
		var (_, navBarHeight) = App.GetAndroidSystemBarInsets();
		var initialBottom = bottomMarkerBefore.Bottom;
		var initialTop = topMarkerBefore.Top;

		Assert.That(initialBottom, Is.EqualTo(rootContainerRect.Bottom - navBarHeight),
			"Bottom marker should be within root container before keyboard appears");

		// Tap the entry to show the keyboard
		App.Tap("TestEntryPan");
		App.WaitForKeyboardToShow();
		Assert.That(App.FindElement("TopLabelPan"), Is.Null,
			"Top label should move up when keyboard appears with AdjustPan, So it will not be accessible");

		var bottomMarkerAfter = App.FindElement("BottomMarkerPan").GetRect();
		var afterBottom = bottomMarkerAfter.Bottom;
		Assert.That(afterBottom, Is.LessThan(initialBottom),
			$"Bottom marker should move up when keyboard appears. Before: {initialBottom}px, After: {afterBottom}px");

		App.DismissKeyboard();
		App.WaitForKeyboardToHide();

		var bottomMarkerFinal = App.FindElement("BottomMarkerPan").GetRect();
		var afterKeyboardCloseBottom = bottomMarkerFinal.Bottom;

		Assert.That(afterKeyboardCloseBottom, Is.EqualTo(initialBottom),
			"Bottom marker should return to initial position after keyboard dismissal");

		var topMarkerFinal = App.FindElement("TopLabelPan").GetRect();
		var afterKeyboardCloseTop = topMarkerFinal.Top;
		Assert.That(afterKeyboardCloseTop, Is.EqualTo(initialTop),
			"Top label should return to initial position after keyboard dismissal");
		Assert.That(rootContainerRect.Bottom, Is.EqualTo(afterKeyboardCloseBottom + navBarHeight));
	}
}
#endif

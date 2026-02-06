#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32041 : _IssuesUITest
{
	public override string Issue => "Keyboard overlaps Entry when SoftInput.AdjustResize is set";

	public Issue32041(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyContainerResizesWithAdjustResize()
	{
		App.WaitForElement("MainContainer");

		var rootContainerRect = App.FindElement("RootPage").GetRect();
		var bottomMarkerBefore = App.FindElement("BottomMarker").GetRect();
		var (_, navBarHeight) = App.GetAndroidSystemBarInsets();
		var topMarker = App.FindElement("TopLabel").GetRect();

		// Verify root container fills screen
		Assert.That(rootContainerRect.Bottom, Is.EqualTo(bottomMarkerBefore.Bottom + navBarHeight));

		var initialBottom = bottomMarkerBefore.Bottom;

		Assert.That(initialBottom, Is.EqualTo(rootContainerRect.Bottom - navBarHeight),
			"Bottom marker should be within root container before keyboard appears");
		var initialTop = topMarker.Top;
		// Show keyboard
		App.Tap("TestEntry");
		App.WaitForKeyboardToShow();

		// With AdjustResize, bottom marker should move up to stay above keyboard
		var afterBottom = App.FindElement("BottomMarker").GetRect().Bottom;
		var afterTop = App.FindElement("TopLabel").GetRect().Top;
		var upwardMovement = initialBottom - afterBottom;
		Assert.That(afterTop, Is.EqualTo(initialTop),
					"Top label position should remain unchanged with AdjustResize");
		Assert.That(afterBottom, Is.LessThan(initialBottom),
			$"Bottom marker should move up when keyboard appears. Before: {initialBottom}px, After: {afterBottom}px");
		Assert.That(upwardMovement, Is.GreaterThan(200),
			$"Bottom marker should move up by at least 200px. Actual: {upwardMovement}px");
		Assert.That(App.FindElement("BottomMarker"), Is.Not.Null,
			"Bottom marker should remain visible after keyboard appears");

		// Dismiss keyboard and verify layout restores
		App.DismissKeyboard();
		App.WaitForKeyboardToHide();

		var afterKeyboardCloseBottom = App.FindElement("BottomMarker").GetRect().Bottom;

		Assert.That(afterKeyboardCloseBottom, Is.EqualTo(initialBottom),
			"Bottom marker should return to initial position after keyboard dismissal");
		Assert.That(rootContainerRect.Bottom, Is.EqualTo(afterKeyboardCloseBottom + navBarHeight));
	}
}
#endif
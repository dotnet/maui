#if ANDROID || IOS  // SafeAreaEdges not supported on Catalyst and Windows

using System.Diagnostics;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33038 : _IssuesUITest
{
	public Issue33038(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Layout breaks on first navigation until soft keyboard appears/disappears";

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void LayoutShouldBeCorrectOnFirstNavigation()
	{
		App.WaitForElement("StartPageLabel");
		App.Tap("GoToSignInButton");

		// On Android the safe-area insets settle over the course of the Shell fragment slide-in
		// animation, so the label keeps moving for a few frames after navigation before it lands
		// at its final position. Reading GetRect() immediately would race that animation and
		// capture a transient value, which is what made this test flaky. Wait for the position to
		// settle before capturing it. On iOS the layout is already settled, so this returns immediately.
		var firstNavigationY = WaitForStableY("SignInLabel");

		// The label must sit below the top safe area (status bar) on first navigation.
		Assert.That(firstNavigationY, Is.GreaterThan(0),
			$"SignInLabel should be positioned below the safe area on first navigation (Y={firstNavigationY}).");

		// Toggling the soft keyboard forces a relayout that always applies the safe area
		// correctly. This gives us the known-correct reference position.
		App.Tap("EmailEntry");
		App.WaitForKeyboardToShow();
		App.DismissKeyboard();
		App.WaitForKeyboardToHide();

		var afterKeyboardToggleY = WaitForStableY("SignInLabel");

		// If the settled first-navigation position matches the post-toggle position, the layout
		// was correct from the start. A mismatch means the layout stayed broken until the
		// keyboard forced a relayout.
		Assert.That(firstNavigationY, Is.EqualTo(afterKeyboardToggleY).Within(1),
			$"SignInLabel Y on first navigation ({firstNavigationY}) should match its Y after a keyboard toggle ({afterKeyboardToggleY}); a difference indicates the layout was broken until the keyboard appeared.");
	}

	// Polls the element's Y position until it stops changing (or a timeout elapses), so we read
	// the settled layout rather than a transient value produced by an in-flight async relayout.
	int WaitForStableY(string automationId)
	{
		var y = App.WaitForElement(automationId).GetRect().Y;
		var stableReads = 0;
		var stopwatch = Stopwatch.StartNew();

		while (stopwatch.ElapsedMilliseconds < 3000)
		{
			Thread.Sleep(150);
			var current = App.WaitForElement(automationId).GetRect().Y;

			if (current == y)
			{
				// Require a few consecutive identical reads so we know the layout has settled.
				if (++stableReads >= 3)
					break;
			}
			else
			{
				stableReads = 0;
				y = current;
			}
		}

		return y;
	}
}
#endif
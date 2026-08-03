#if ANDROID // Test validates Android-specific IME/safe-area behavior with Android-only instrumentation in the HostApp page
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue37012 : _IssuesUITest
{
	public Issue37012(TestDevice device) : base(device) { }

	public override string Issue => "Safe-area padding stale for the whole IME hide animation when swapping pages";

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void SafeAreaPaddingAppliedToPageReattachedDuringImeHideAnimation()
	{
		App.WaitForElement("OpenPageB");
		App.Tap("OpenPageB");

		App.WaitForElement("PageBEntry");
		App.Tap("PageBEntry");

		// Ensure the keyboard is fully shown before triggering the hide + pop sequence
		App.WaitForKeyboardToShow();

		App.Tap("HideAndPopButton");

		// Page A reports its safe-area padding captured on the first frame drawn after
		// re-attach vs. the padding once the IME hide animation has completed.
		App.WaitForElement("ResultLabel");
		var success = App.WaitForTextToBePresentInElement("ResultLabel", "Success", timeout: TimeSpan.FromSeconds(5));
		var resultText = App.FindElement("ResultLabel").GetText();
		Assert.That(success, Is.True, $"Expected safe-area padding on the first frame after re-attach, but got: {resultText}");
	}
}
#endif

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
		// The scenario races a page pop against the IME hide animation; the page verifies the
		// race was actually won (re-attach happened mid-animation) and reports Inconclusive
		// otherwise, so retry a few times before giving up as inconclusive.
		string resultText = string.Empty;
		for (int attempt = 1; attempt <= 3; attempt++)
		{
			App.WaitForElement("OpenPageB");
			App.Tap("OpenPageB");

			App.WaitForElement("PageBEntry");
			App.Tap("PageBEntry");

			// Ensure the keyboard is fully shown before triggering the hide + pop sequence.
			// This returns false on timeout rather than throwing, and without the keyboard
			// there is no IME animation to race — which would otherwise be indistinguishable
			// from a genuinely lost race and silently report Inconclusive forever.
			Assert.That(App.WaitForKeyboardToShow(TimeSpan.FromSeconds(5)), Is.True,
				"Keyboard never appeared; the IME-hide race cannot be exercised on this device.");

			App.Tap("HideAndPopButton");

			// Page A reports its safe-area padding captured on the first frame drawn after
			// re-attach vs. the padding once the IME hide animation completed and settled.
			App.WaitForElement("ResultLabel");
			resultText = WaitForResult();

			if (resultText.StartsWith("Success", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			if (!resultText.StartsWith("Inconclusive", StringComparison.OrdinalIgnoreCase))
			{
				break;
			}
		}

		if (resultText.StartsWith("Inconclusive", StringComparison.OrdinalIgnoreCase))
		{
			Assert.Inconclusive($"Could not reproduce the mid-animation re-attach precondition: {resultText}");
		}

		Assert.Fail($"Expected safe-area padding on the first frame after re-attach, but got: {resultText}");
	}

	string WaitForResult()
	{
		// The page writes Success/Fail/Inconclusive when its polling loop settles (up to ~4s
		// on its side); poll the label instead of waiting a fixed time
		var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(15);
		string text;
		do
		{
			text = App.FindElement("ResultLabel").GetText() ?? string.Empty;
			if (!string.IsNullOrEmpty(text) && text != "Waiting")
			{
				return text;
			}

			Thread.Sleep(500);
		}
		while (DateTime.UtcNow < deadline);

		return text;
	}
}
#endif

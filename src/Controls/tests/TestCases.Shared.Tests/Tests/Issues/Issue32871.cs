#if ANDROID // Android-only: fix and native padding assertion rely on Android platform code
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32871 : _IssuesUITest
{
	public override string Issue => "[Android] Bottom insets issues when keyboard is shown";

	public Issue32871(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void BottomPaddingShouldBePreservedWhileKeyboardIsShowing()
	{
		App.WaitForElement("MainGrid");
		App.WaitForTextToBePresentInElement("PaddingLabel", "NativePadding");

		var initialPaddingText = App.FindElement("PaddingLabel").GetText() ?? "";
		var initialBottomPadding = ExtractBottomPadding(initialPaddingText);

		if (initialBottomPadding <= 0)
		{
			Assert.Ignore("Device has no navigation bar bottom inset — cannot validate this regression.");
			return;
		}

		App.Tap("TestEntry");

		Assert.That(App.WaitForKeyboardToShow(), Is.True,
			"Keyboard must be visible to validate the fix.");

		var paddingWhileKeyboard = App.FindElement("PaddingLabel").GetText() ?? "";
		var bottomPaddingDuringKeyboard = ExtractBottomPadding(paddingWhileKeyboard);

		Assert.That(bottomPaddingDuringKeyboard, Is.EqualTo(initialBottomPadding),
			$"Bottom padding should be preserved while keyboard is showing. " +
			$"Initial: {initialBottomPadding}px, During keyboard: {bottomPaddingDuringKeyboard}px.");
	}

	static double ExtractBottomPadding(string paddingText)
	{
		var prefix = "B=";
		var idx = paddingText.IndexOf(prefix, StringComparison.Ordinal);
		if (idx >= 0)
		{
			var valueStr = paddingText.Substring(idx + prefix.Length).Trim();
			if (double.TryParse(valueStr, out var value))
				return value;
		}
		return -1;
	}
}
#endif

// The focus-loss ordering being observed is specific to Android.
#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38799 : _IssuesUITest
{
	public Issue38799(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Replacing focused Entry content on a Shell page";

	[Test]
	[ShardedTestCategory(UITestCategories.Shell)]
	public void ReplacingTextEntryRelinquishesFocusBeforeRemoval()
	{
		App.WaitForElement("TextCell");
		App.Tap("TextCell");
		AssertNativeEditorAndKeyboard("TextEntry");

		for (int replacement = 1; replacement <= 2; replacement++)
		{
			App.WaitForElement("NumericCell");
			App.Tap("NumericCell");
			Assert.That(App.WaitForTextToBePresentInElement("FocusResult",
				$"{replacement}: Focus relinquished before removal"), Is.True);
			AssertNativeEditorAndKeyboard("NumericEntry");

			App.WaitForElement("TextCell");
			App.Tap("TextCell");
			AssertNativeEditorAndKeyboard("TextEntry");
		}
	}

	void AssertNativeEditorAndKeyboard(string automationId)
	{
		App.WaitForElement(automationId);
		// The original sample requests focus before insertion; tap after attachment as well.
		App.Tap(automationId);
		Assert.That(App.WaitForTextToBePresentInElement("NativeState",
			$"{automationId}: Native focus and input type correct"), Is.True,
			"The incoming Entry must have native focus and the expected input class.");
		Assert.That(App.WaitForKeyboardToShow(), Is.True, $"{automationId} must show the keyboard.");
	}
}
#endif

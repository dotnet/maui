#if ANDROID // Key event dispatch is Android-specific behavior
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21013 : _IssuesUITest
{
	public override string Issue => "OnKeyUp OnKeyDown dispatchKeyEvent not firing when back button pressed with keyboard open";

	public Issue21013(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Entry)]
	public void EntryShouldLoseFocusWhenBackButtonDismissesKeyboard()
	{
		App.WaitForElement("TestEntry");

		// Focus the entry to show keyboard
		App.Tap("TestEntry");
		App.WaitForKeyboardToShow(timeout: TimeSpan.FromSeconds(3));

		// Verify entry is focused
		var focusBefore = App.FindElement("FocusStatusLabel").GetText();
		Assert.That(focusBefore, Is.EqualTo("IsFocused: true"));

		App.Back();

		// Wait for keyboard to hide
		App.WaitForKeyboardToHide(timeout: TimeSpan.FromSeconds(3));

		// Verify entry lost focus (ClearFocus was called by OnKeyPreIme fix)
		var focusAfter = App.WaitForElement("FocusStatusLabel").GetText();
		Assert.That(focusAfter, Is.EqualTo("IsFocused: false"));
	}
}
#endif

#if ANDROID // TextView skipping the soft keyboard request when its layout is invalidated on focus is Android-specific
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38819 : _IssuesUITest
{
	public Issue38819(TestDevice device) : base(device) { }

	public override string Issue => "Modifying FontAttributes on focus prevents the keyboard from appearing";

	[Test]
	[Category(UITestCategories.Entry)]
	public void KeyboardAppearsOnFirstTapWhenEntryFontChangesOnFocus()
	{
		App.WaitForElement("InstructionsLabel");

		if (App.IsKeyboardShown())
			App.DismissKeyboard();

		App.WaitForElement("FontChangeOnFocusEntry");
		App.Tap("FontChangeOnFocusEntry");

		Assert.That(App.IsKeyboardShown(), Is.True,
			"The keyboard should appear on the first tap even though a trigger changes FontAttributes when the Entry gains focus.");

		App.DismissKeyboard();
	}

	[Test]
	[Category(UITestCategories.Editor)]
	public void KeyboardAppearsOnFirstTapWhenEditorFontChangesOnFocus()
	{
		App.WaitForElement("InstructionsLabel");

		if (App.IsKeyboardShown())
			App.DismissKeyboard();

		App.WaitForElement("FontChangeOnFocusEditor");
		App.Tap("FontChangeOnFocusEditor");

		Assert.That(App.IsKeyboardShown(), Is.True,
			"The keyboard should appear on the first tap even though a trigger changes FontAttributes when the Editor gains focus.");

		App.DismissKeyboard();
	}
}
#endif

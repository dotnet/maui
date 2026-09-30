#if ANDROID // TextView skipping the rest of a tap or long press when its layout is invalidated on focus is Android-specific
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38819 : _IssuesUITest
{
	const string Text = "1234567890";

	public Issue38819(TestDevice device) : base(device) { }

	public override string Issue => "Modifying FontAttributes on focus prevents the keyboard from appearing";

	// Every test starts from an unfocused control, because focusing it is what changes the font
	protected override bool ResetAfterEachTest => true;

	[Test]
	[Category(UITestCategories.Entry)]
	public void KeyboardAppearsOnFirstTapWhenEntryFontChangesOnFocus()
	{
		App.WaitForElement("FontChangeOnFocusEntry");
		App.Tap("FontChangeOnFocusEntry");

		Assert.That(App.IsKeyboardShown(), Is.True,
			"The keyboard should appear on the first tap even though a trigger changes FontAttributes when the Entry gains focus.");
	}

	[Test]
	[Category(UITestCategories.Editor)]
	public void KeyboardAppearsOnFirstTapWhenEditorFontChangesOnFocus()
	{
		App.WaitForElement("FontChangeOnFocusEditor");
		App.Tap("FontChangeOnFocusEditor");

		Assert.That(App.IsKeyboardShown(), Is.True,
			"The keyboard should appear on the first tap even though a trigger changes FontAttributes when the Editor gains focus.");
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void FirstTapPlacesCursorWhenEntryFontChangesOnFocus()
	{
		var rect = App.WaitForElement("FontChangeOnFocusEntry").GetRect();

		// Tapping past the end of the text puts the cursor at the end of the text
		App.TapCoordinates(rect.X + rect.Width - 20, rect.Y + rect.Height / 2);

		Assert.That(App.WaitForElement("EntrySelectionLabel").GetText(), Is.EqualTo($"{Text.Length},0"),
			"The first tap should place the cursor where the Entry was tapped.");
	}

	[Test]
	[Category(UITestCategories.Editor)]
	public void FirstTapPlacesCursorWhenEditorFontChangesOnFocus()
	{
		var rect = App.WaitForElement("FontChangeOnFocusEditor").GetRect();

		// The text is on the first line, so tapping past its end on that line puts the cursor at the end of the text
		App.TapCoordinates(rect.X + rect.Width - 20, rect.Y + 40);

		Assert.That(App.WaitForElement("EditorSelectionLabel").GetText(), Is.EqualTo($"{Text.Length},0"),
			"The first tap should place the cursor where the Editor was tapped.");
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void LongPressSelectsWordWhenEntryFontChangesOnFocus()
	{
		var rect = App.WaitForElement("FontChangeOnFocusEntry").GetRect();

		// Long pressing on the text selects the word under the finger, which is the whole text
		App.TouchAndHoldCoordinates(rect.X + rect.Width / 4, rect.Y + rect.Height / 2);

		Assert.That(App.WaitForElement("EntrySelectionLabel").GetText(), Is.EqualTo($"0,{Text.Length}"),
			"A long press should select the word even though a trigger changes FontAttributes when the Entry gains focus.");
	}

	[Test]
	[Category(UITestCategories.Editor)]
	public void LongPressSelectsWordWhenEditorFontChangesOnFocus()
	{
		var rect = App.WaitForElement("FontChangeOnFocusEditor").GetRect();

		// Long pressing on the text selects the word under the finger, which is the whole text
		App.TouchAndHoldCoordinates(rect.X + rect.Width / 4, rect.Y + 40);

		Assert.That(App.WaitForElement("EditorSelectionLabel").GetText(), Is.EqualTo($"0,{Text.Length}"),
			"A long press should select the word even though a trigger changes FontAttributes when the Editor gains focus.");
	}
}
#endif

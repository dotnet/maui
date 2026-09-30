#if ANDROID // TextView skipping its tap, long press or accessibility click handling when the layout is invalidated on focus is Android-specific
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38819 : _IssuesUITest
{
	const string Text = "1234567890";

	// Must match Issue38819AutofillService in the HostApp
	const string AutofillService = "com.microsoft.maui.uitests/com.microsoft.maui.uitests.Issue38819AutofillService";
	const string AutofillHint = "maui-issue-38819";
	const string AutofillDialogHeader = "Issue38819 fill dialog";
	const string AutofillSuggestion = "Issue38819 suggestion";

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

		Assert.That(App.WaitForKeyboardToShow(), Is.True,
			"The keyboard should appear on the first tap even though a trigger changes FontAttributes when the Entry gains focus.");
	}

	[Test]
	[Category(UITestCategories.Editor)]
	public void KeyboardAppearsOnFirstTapWhenEditorFontChangesOnFocus()
	{
		App.WaitForElement("FontChangeOnFocusEditor");
		App.Tap("FontChangeOnFocusEditor");

		Assert.That(App.WaitForKeyboardToShow(), Is.True,
			"The keyboard should appear on the first tap even though a trigger changes FontAttributes when the Editor gains focus.");
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void KeyboardAppearsOnAccessibilityClickWhenEntryFontChangesOnFocus()
	{
		// TalkBack activates an Entry with ACTION_CLICK, which TextView handles like a tap
		App.WaitForElement("AccessibilityClickEntryButton");
		App.Tap("AccessibilityClickEntryButton");

		Assert.That(App.WaitForKeyboardToShow(), Is.True,
			"The keyboard should appear when the Entry is activated through accessibility even though a trigger changes FontAttributes when it gains focus.");
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

	[Test]
	[Category(UITestCategories.Entry)]
	public void FirstTapKeepsNativeAutofillBehaviorWhenEntryFontChangesOnFocus()
	{
		// TextView decides between the autofill fill dialog and the keyboard in its tap handling only on API 33+
		if (App is AppiumApp appiumApp)
		{
			var apiLevel = (long?)appiumApp.Driver.Capabilities.GetCapability("deviceApiLevel") ?? 0;
			if (apiLevel < 33)
			{
				Assert.Ignore($"The autofill fill dialog requires Android API 33+. Current device API: {apiLevel}.");
			}
		}

		var previousAutofillService = ShellHelper.ExecuteShellCommandWithOutput("adb shell settings get secure autofill_service").Trim();

		try
		{
			ShellHelper.ExecuteShellCommand($"adb shell settings put secure autofill_service {AutofillService}");
			ShellHelper.ExecuteShellCommand($"adb shell device_config put autofill autofill_dialog_hints {AutofillHint}");

			// Whether Android also shows the keyboard with the fill dialog depends on the API level,
			// so compare with the same Entry without the font trigger
			var nativeKeyboardShown = TapAndChooseAutofillSuggestion("AutofillReferenceEntry");
			var keyboardShown = TapAndChooseAutofillSuggestion("FontChangeOnFocusAutofillEntry");

			Assert.That(keyboardShown, Is.EqualTo(nativeKeyboardShown),
				"The keyboard should be shown or suppressed with the fill dialog as it is for an Entry without the font trigger.");
		}
		finally
		{
			ShellHelper.ExecuteShellCommand("adb shell device_config delete autofill autofill_dialog_hints");
			ShellHelper.ExecuteShellCommand(previousAutofillService is "" or "null"
				? "adb shell settings delete secure autofill_service"
				: $"adb shell settings put secure autofill_service {previousAutofillService}");
		}
	}

	// Returns whether the keyboard was shown together with the fill dialog
	bool TapAndChooseAutofillSuggestion(string entryAutomationId)
	{
		// Restart the app so it reads the fill dialog settings and the Entry starts unfocused
		Reset();
		FixtureSetup();

		App.WaitForElement(entryAutomationId);
		App.Tap(entryAutomationId);

		App.WaitForElement(AppiumQuery.ByXPath($"//*[@text='{AutofillDialogHeader}']"), $"The fill dialog should appear on the first tap on {entryAutomationId}.");
		var keyboardShown = App.WaitForKeyboardToShow(TimeSpan.FromSeconds(2));

		App.Tap(AppiumQuery.ByXPath($"//*[@text='{AutofillSuggestion}']"));

		Assert.That(App.WaitForElement(entryAutomationId).GetText(), Is.EqualTo("Autofilled"),
			$"Choosing the suggestion in the fill dialog should autofill {entryAutomationId}.");

		return keyboardShown;
	}
}
#endif

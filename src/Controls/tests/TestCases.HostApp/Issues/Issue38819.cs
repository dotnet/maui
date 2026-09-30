namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38819, "Modifying FontAttributes on focus prevents the keyboard from appearing", PlatformAffected.Android)]
public class Issue38819 : ContentPage
{
	public Issue38819()
	{
		var entry = new Entry
		{
			AutomationId = "FontChangeOnFocusEntry",
			Text = "1234567890",
			WidthRequest = 300,
			HorizontalOptions = LayoutOptions.Center,
			FontAttributes = FontAttributes.Italic,
		};
		entry.Triggers.Add(CreateFontAttributesOnFocusTrigger(typeof(Entry), Entry.FontAttributesProperty));

		var editor = new Editor
		{
			AutomationId = "FontChangeOnFocusEditor",
			Text = "1234567890",
			WidthRequest = 300,
			HeightRequest = 100,
			HorizontalOptions = LayoutOptions.Center,
			FontAttributes = FontAttributes.Italic,
		};
		editor.Triggers.Add(CreateFontAttributesOnFocusTrigger(typeof(Editor), Editor.FontAttributesProperty));

		// While the UI test makes Issue38819AutofillService the autofill service, tapping these Entries shows a fill dialog.
		// The second one has no font trigger, so the test can compare with what Android does natively.
		var autofillEntry = CreateAutofillEntry("FontChangeOnFocusAutofillEntry", "Autofill, font changes on focus");
		autofillEntry.Triggers.Add(CreateFontAttributesOnFocusTrigger(typeof(Entry), Entry.FontAttributesProperty));
		var autofillReferenceEntry = CreateAutofillEntry("AutofillReferenceEntry", "Autofill, no font trigger");

		// Activates the Entry the way TalkBack does (ACTION_CLICK), which TextView handles like a tap outside of a touch event
		var accessibilityClickButton = new Button
		{
			AutomationId = "AccessibilityClickEntryButton",
			Text = "Activate the Entry with an accessibility click",
		};
#if ANDROID
		accessibilityClickButton.Clicked += (_, _) =>
			(entry.Handler?.PlatformView as Android.Views.View)?.PerformAccessibilityAction(Android.Views.Accessibility.Action.Click, null);
#endif

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children =
			{
				new Label
				{
					AutomationId = "InstructionsLabel",
					Text = "Tap or long press the Entry or the Editor once. The keyboard should appear, and the cursor or selection should be where you touched."
				},
				entry,
				CreateSelectionLabel(entry, "EntrySelectionLabel"),
				editor,
				CreateSelectionLabel(editor, "EditorSelectionLabel"),
				autofillEntry,
				autofillReferenceEntry,
				accessibilityClickButton,
			}
		};
	}

	// Changing the font when the control gains focus invalidates the Android text layout
	// in the middle of the tap, long press or accessibility click that focused it.
	static Trigger CreateFontAttributesOnFocusTrigger(Type targetType, BindableProperty fontAttributesProperty)
	{
		var trigger = new Trigger(targetType)
		{
			Property = VisualElement.IsFocusedProperty,
			Value = true,
		};
		trigger.Setters.Add(new Setter { Property = fontAttributesProperty, Value = FontAttributes.None });
		return trigger;
	}

	static Entry CreateAutofillEntry(string automationId, string placeholder)
	{
		var entry = new Entry
		{
			AutomationId = automationId,
			Placeholder = placeholder,
			WidthRequest = 300,
			HorizontalOptions = LayoutOptions.Center,
			FontAttributes = FontAttributes.Italic,
		};
#if ANDROID
		entry.HandlerChanged += (_, _) =>
		{
			if (entry.Handler?.PlatformView is Android.Views.View view && OperatingSystem.IsAndroidVersionAtLeast(26))
			{
				view.SetAutofillHints(Platform.Issue38819AutofillService.AutofillHint);
			}
		};
#endif
		return entry;
	}

	// Shows "CursorPosition,SelectionLength" so the tests can check where the touch placed the cursor or selection.
	static Label CreateSelectionLabel(InputView inputView, string automationId)
	{
		var label = new Label { AutomationId = automationId };

		void Update() => label.Text = $"{inputView.CursorPosition},{inputView.SelectionLength}";

		inputView.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(InputView.CursorPosition) || e.PropertyName == nameof(InputView.SelectionLength))
				Update();
		};
		Update();

		return label;
	}
}

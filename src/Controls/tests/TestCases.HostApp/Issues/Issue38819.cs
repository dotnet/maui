namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38819, "Modifying FontAttributes on focus prevents the keyboard from appearing", PlatformAffected.Android)]
public class Issue38819 : ContentPage
{
	public Issue38819()
	{
		var entry = new Entry
		{
			AutomationId = "FontChangeOnFocusEntry",
			Text = "1234",
			WidthRequest = 200,
			HorizontalOptions = LayoutOptions.Center,
			FontAttributes = FontAttributes.Italic,
		};
		entry.Triggers.Add(CreateFontAttributesOnFocusTrigger(typeof(Entry), Entry.FontAttributesProperty));

		var editor = new Editor
		{
			AutomationId = "FontChangeOnFocusEditor",
			Text = "1234",
			WidthRequest = 200,
			HeightRequest = 100,
			HorizontalOptions = LayoutOptions.Center,
			FontAttributes = FontAttributes.Italic,
		};
		editor.Triggers.Add(CreateFontAttributesOnFocusTrigger(typeof(Editor), Editor.FontAttributesProperty));

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children =
			{
				new Label
				{
					AutomationId = "InstructionsLabel",
					Text = "Tap the Entry or the Editor once. The keyboard should appear on the first tap."
				},
				entry,
				editor,
			}
		};
	}

	// Changing the font when the control gains focus invalidates the Android text layout
	// in the middle of the touch event that focused it.
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
}

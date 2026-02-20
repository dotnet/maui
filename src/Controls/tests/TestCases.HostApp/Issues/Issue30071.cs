namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, "30071", "Entry Editor Placeholder CharacterSpacing Property Not Working on Windows", PlatformAffected.UWP)]

public class Issue30071 : ContentPage
{
	public Issue30071()
	{
		var label = new Label
		{
			Text = "Test passes if Entry and Editor Placeholder has character Spacing",
			AutomationId = "label",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};

		var entry = new Entry
		{
			Placeholder = "Enter text here",
			CharacterSpacing = 10,
			AutomationId = "entry",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};

		var editor = new Editor
		{
			Placeholder = "Enter text here",
			CharacterSpacing = 10,
			HeightRequest = 100,
			WidthRequest = 300,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};

		var button = new Button
		{
			Text = "Change Character Spacing to 20",
			AutomationId = "button",
			HorizontalOptions = LayoutOptions.Center,
			Margin = 10
		};

		button.Clicked += (s, e) =>
		{
			entry.CharacterSpacing = 20;
			editor.CharacterSpacing = 20;
		};

		Content = new StackLayout
		{
			Children = { label, entry, editor, button },
			Spacing = 20,
			Margin = new Thickness(20)
		};
	}
}
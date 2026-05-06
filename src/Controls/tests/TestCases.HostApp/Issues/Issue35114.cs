namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35114, "Editor can not be scrolled after rotating simulator", PlatformAffected.iOS)]
public class Issue35114 : ContentPage
{
	public Issue35114()
	{
		Title = "Issue 35114";

		var slider = new Slider
		{
			AutomationId = "Slider",
			Maximum = 300,
			Minimum = 0
		};

		var editor = new Editor
		{
			AutomationId = "TestEditor",
			Text = "testing"
		};

		editor.BindingContext = slider;
		editor.SetBinding(Editor.CharacterSpacingProperty, new Binding("Value"));

		Content = new VerticalStackLayout
		{
			Children =
			{
				new Label { Text = "1. Play with the value of the slider below and observe as the space between characters widens." },
				new Label { Text = "2. The tests fails if the space between characters does not change." },
				slider,
				editor
			}
		};
	}
}

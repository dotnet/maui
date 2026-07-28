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
				new Label { Text = "1. Drag the slider to the right to increase character spacing on the Editor." },
				new Label { Text = "2. Rotate the device to landscape and back to portrait." },
				new Label { Text = "3. The test fails if the Editor grows to full content height after rotation (it should remain scrollable)." },
				slider,
				editor
			}
		};
	}
}

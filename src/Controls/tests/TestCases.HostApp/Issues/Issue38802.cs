namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38802, "Editor should be scrolled after rotating simulator", PlatformAffected.iOS)]
public class Issue38802 : ContentPage
{
    public Issue38802()
    {
        Title = "Issue 38802";

        var slider = new Slider
        {
            AutomationId = "Issue38802Slider",
            Maximum = 150,
            Minimum = 0
        };

        var editor = new Editor
        {
            AutomationId = "Issue38802TestEditor",
            BackgroundColor = Colors.Yellow,
            Text = "test"
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

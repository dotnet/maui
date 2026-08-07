namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19099, "TapGestureRecognizer no longer works on Button", PlatformAffected.iOS)]
public class Issue19099 : ContentPage
{
    public Issue19099()
    {
        var resultLabel = new Label
        {
            AutomationId = "GestureResultLabel",
            Text = "Not tapped"
        };

        var gestureButton = new Button
        {
            AutomationId = "GestureButton",
            Text = "Tap me"
        };

        gestureButton.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() => resultLabel.Text = "Tapped")
        });

        Content = new VerticalStackLayout
        {
            Padding = new Thickness(20),
            Spacing = 12,
            Children =
            {
                new Label
                {
                    Text = "Tap the button. The test passes when the TapGestureRecognizer command updates the result label.",
                    AutomationId = "InstructionLabel"
                },
                gestureButton,
                resultLabel
            }
        };
    }
}

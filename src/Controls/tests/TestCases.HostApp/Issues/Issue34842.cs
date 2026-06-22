namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34842, "[iOS] RadioButton BackgroundColor bleeds outside CornerRadius rounded corners", PlatformAffected.iOS)]
public class Issue34842 : ContentPage
{
    public Issue34842()
    {
        RadioButton radioButton = new RadioButton
        {
            Content = "Option A",
            BackgroundColor = Colors.LightBlue,
            WidthRequest = 200,
            CornerRadius = 15,
            BorderColor = Colors.Black,
            BorderWidth = 2,
            AutomationId = "RadioButtonWithBackground"
        };

        Content = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 16,
            BackgroundColor = Colors.White,
            Children =
            {
                new Label
                {
                    Text = "The test passes if the RadioButton background color is clipped to its rounded corners and does not bleed outside.",
                    AutomationId = "InstructionLabel"
                },
                radioButton
            }
        };
    }
}

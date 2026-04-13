namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30192, "TimePicker FlowDirection Not Working on All Platforms", PlatformAffected.All)]
public class Issue30192 : ContentPage
{
    public Issue30192()
    {
        TimePicker ltrTimePicker = new TimePicker
        {
            Time = new TimeSpan(2, 0, 0),
            Format = "hh:mm tt",
            FlowDirection = FlowDirection.LeftToRight,
        };

        TimePicker rtlTimePicker = new TimePicker
        {
            Time = new TimeSpan(2, 0, 0),
            Format = "hh:mm tt",
            FlowDirection = FlowDirection.RightToLeft,
        };

        Button button = new Button
        {
            Text = "Toggle FlowDirection",
            AutomationId = "ToggleFlowDirectionButton"
        };
        button.Clicked += (sender, e) =>
        {
            ltrTimePicker.FlowDirection = FlowDirection.RightToLeft;
            rtlTimePicker.FlowDirection = FlowDirection.LeftToRight;
        };

        Content = new VerticalStackLayout
        {
            Children =
                {
                    ltrTimePicker,
                    rtlTimePicker,
                    button
                }
        };
    }
}
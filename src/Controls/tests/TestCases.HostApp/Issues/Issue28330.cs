namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 28330, "Stepper allows to increment when value equals to maximum", PlatformAffected.All)]
public class Issue28330 : TestContentPage
{
    protected override void Init()
    {
        var layout = new StackLayout { };

        Stepper stepper = new Stepper
        {
            AutomationId = "stepper",
            HorizontalOptions = LayoutOptions.Center,
            Increment = 1,
            Minimum = 1,
            Maximum = 1,
            Value = 1
        };

        Label label = new Label
        {
            AutomationId = "label",
            HorizontalOptions = LayoutOptions.Center,
            FontSize = 32
        };
        label.SetBinding(Label.TextProperty, new Binding("Value", source: stepper));

        layout.Children.Add(stepper);
        layout.Children.Add(label);

        Content = layout;
    }
}
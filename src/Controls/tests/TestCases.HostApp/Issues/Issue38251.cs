namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38251, "Button RTL image and text overlap on iOS", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue38251 : ContentPage
{
    public Issue38251()
    {
        var grid = new Grid
        {
            Padding = 24,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
            },
            RowSpacing = 12,
        };

        grid.Add(CreateLabel("LTR reference (image right)"), 0, 1);

        grid.Add(CreateButton("LtrReferenceButton", new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Right, 50), FlowDirection.LeftToRight), 0, 2);

        grid.Add(CreateLabel("RTL reproduction (image right)"), 0, 3);

        grid.Add(CreateButton("RtlReferenceButton", new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Right, 50), FlowDirection.RightToLeft), 0, 4);

        grid.Add(CreateLabel("LTR reference (image left)"), 0, 5);

        grid.Add(CreateButton("LtrLeftImageButton", new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 50), FlowDirection.LeftToRight), 0, 6);

        grid.Add(CreateLabel("RTL reproduction (image left)"), 0, 7);

        grid.Add(CreateButton("RtlLeftImageButton", new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 50), FlowDirection.RightToLeft), 0, 8);

        Content = grid;
    }

    Button CreateButton(string automationId, Button.ButtonContentLayout contentLayout, FlowDirection flowDirection)
    {
        Button button = new Button
        {
            AutomationId = automationId,
            BackgroundColor = Colors.LightGray,
            Text = "Image1",
            ImageSource = "coffee.png",
            ContentLayout = contentLayout,
            FlowDirection = flowDirection,
            HorizontalOptions = LayoutOptions.Fill,
        };
        return button;
    }

    Label CreateLabel(string text)
    {
        return new Label
        {
            Text = text,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0, 12, 0, 0),
        };
    }
}
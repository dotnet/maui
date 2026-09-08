namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38251, "Button RTL image and text overlap on iOS", PlatformAffected.iOS)]
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

        grid.Add(new Label
        {
            Text = "LTR reference (image right)",
            FontAttributes = FontAttributes.Bold,
        }, 0, 1);

        grid.Add(new Button
        {
            AutomationId = "LtrReferenceButton",
            Text = "Image1",
            ImageSource = "coffee.png",
            ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Right, 50),
            FlowDirection = FlowDirection.LeftToRight,
            HorizontalOptions = LayoutOptions.Fill,
        }, 0, 2);

        grid.Add(new Label
        {
            Margin = new Thickness(0, 12, 0, 0),
            Text = "RTL reproduction (image right)",
            FontAttributes = FontAttributes.Bold,
        }, 0, 3);

        grid.Add(new Button
        {
            AutomationId = "RtlReferenceButton",
            Text = "Image1",
            ImageSource = "coffee.png",
            ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Right, 50),
            FlowDirection = FlowDirection.RightToLeft,
            HorizontalOptions = LayoutOptions.Fill,
        }, 0, 4);

        grid.Add(new Label
        {
            Margin = new Thickness(0, 12, 0, 0),
            Text = "LTR reference (image left)",
            FontAttributes = FontAttributes.Bold,
        }, 0, 5);

        grid.Add(new Button
        {
            AutomationId = "LtrLeftImageButton",
            Text = "Image1",
            ImageSource = "coffee.png",
            ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 50),
            FlowDirection = FlowDirection.LeftToRight,
            HorizontalOptions = LayoutOptions.Fill,
        }, 0, 6);

        grid.Add(new Label
        {
            Margin = new Thickness(0, 12, 0, 0),
            Text = "RTL reproduction (image left)",
            FontAttributes = FontAttributes.Bold,
        }, 0, 7);

        grid.Add(new Button
        {
            AutomationId = "RtlLeftImageButton",
            Text = "Image1",
            ImageSource = "coffee.png",
            ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 50),
            FlowDirection = FlowDirection.RightToLeft,
            HorizontalOptions = LayoutOptions.Fill,
        }, 0, 8);

        Content = grid;
    }
}
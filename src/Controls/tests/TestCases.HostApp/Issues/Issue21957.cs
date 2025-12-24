namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21957, "Views with explicit size larger than screen do not respect margins", PlatformAffected.All)]
public class Issue21957 : ContentPage
{
    public Issue21957()
    {
        var stackLayout = new StackLayout
        {
            Margin = new Thickness(30),
            WidthRequest = 450,
            HeightRequest = 200,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Yellow
        };

        stackLayout.Children.Add(new Label
        {
            Text = "StackLayout with Margin 30",
            AutomationId = "YellowStack",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.CenterAndExpand
        });

        var grid = new Grid
        {
            Margin = new Thickness(30),
            WidthRequest = 450,
            HeightRequest = 200,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Green
        };

        grid.Add(new Label
        {
            Text = "Grid with Margin 30",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.CenterAndExpand
        });

        Content = new VerticalStackLayout
        {
            Children = { stackLayout, grid }
        };
    }
}
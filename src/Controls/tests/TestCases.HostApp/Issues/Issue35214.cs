namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35214, "Dynamically changing IndicatorView IndicatorSize to default value does not work", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue35214 : ContentPage
{
    public Issue35214()
    {
        var carouselItems = new List<string> { "Item 0", "Item 1", "Item 2" };

        var indicatorView = new IndicatorView
        {
            AutomationId = "TestIndicatorView",
            HorizontalOptions = LayoutOptions.Center,
            IndicatorColor = Colors.Orange,
            SelectedIndicatorColor = Colors.Blue,
            IndicatorSize = 20,
        };

        var carouselView = new CarouselView
        {
            ItemsSource = carouselItems,
            HeightRequest = 300,
            HorizontalOptions = LayoutOptions.Fill,
            IndicatorView = indicatorView,
            ItemTemplate = new DataTemplate(() =>
            {
                var label = new Label
                {
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    FontSize = 18,
                };
                label.SetBinding(Label.TextProperty, ".");
                return label;
            }),
        };

        var setDefaultSizeButton = new Button
        {
            AutomationId = "SetDefaultSizeButton",
            Text = "Set IndicatorSize = 6 (Default)",
            Margin = new Thickness(0, 20, 0, 0)
        };

        setDefaultSizeButton.Clicked += (s, e) =>
        {
            indicatorView.IndicatorSize = 6;
        };

        Content = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 10,
            Children =
            {
                carouselView,
                indicatorView,
                setDefaultSizeButton,
            }
        };
    }
}

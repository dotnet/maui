namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32136, "CarouselView CurrentItem Not Updating with Vertical LinearItemsLayout", PlatformAffected.iOS)]
public class Issue32136 : ContentPage
{
    public Issue32136()
    {
        CarouselView1 carouselView = new CarouselView1
        {
            HeightRequest = 400,
            Loop = false,
            BackgroundColor = Colors.LightGray,
            AutomationId = "TestCarouselView",
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical),
            ItemTemplate = new DataTemplate(() =>
            {
                Label label = new Label
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
                label.SetBinding(Label.TextProperty, ".");

                return new Grid
                {
                    Children = { label }
                };
            }),
            ItemsSource = new string[]
            {
                "Baboon",
                "Capuchin Monkey",
                "Blue Monkey",
                "Squirrel Monkey",
                "Golden Lion Tamarin"
            }
        };

        Label currentItemLabel = new Label
        {
            AutomationId = "CurrentItemLabel"
        };
        currentItemLabel.SetBinding(Label.TextProperty, ".");

        carouselView.CurrentItemChanged += (s, e) =>
        {
            currentItemLabel.Text = $"CurrentItem = {e.CurrentItem}";
        };

        Grid grid = new Grid
        {
            Padding = 25,
            RowSpacing = 10,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            }
        };

        grid.Add(carouselView);
        grid.Add(currentItemLabel, row: 1);
        Content = grid;
    }
}

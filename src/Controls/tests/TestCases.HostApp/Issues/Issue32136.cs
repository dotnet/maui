namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32136, "CarouselView CurrentItem Not Updating with Vertical LinearItemsLayout", PlatformAffected.iOS)]
public class Issue32136 : ContentPage
{
    public Issue32136()
    {
        CarouselView2 carouselView = new CarouselView2
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

        Label currentItemLabel = new Label();
        currentItemLabel.AutomationId = "CurrentItemLabel";
        currentItemLabel.Text = "CurrentItem = Baboon";

        Button button = new Button
        {
            Text = "Next Item",
            AutomationId = "ScrollButton"
        };
        button.Clicked += (s, e) =>
        {
            currentItemLabel.Text = "Button was clicked";
            carouselView.ScrollTo(carouselView.Position + 1, position: ScrollToPosition.Center, animate: true);
        };

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
        grid.Add(button, row: 2);
        Content = grid;
    }
}

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32048, "CurrentItem does not update when ItemSpacing is set", PlatformAffected.iOS)]
public class Issue32048 : ContentPage
{
    CarouselView2 carouselView;
    Label currentItemLabel;
    string firstItem = "Baboon";

    public Issue32048()
    {
        carouselView = new CarouselView2
        {
            AutomationId = "CarouselViewWithItemSpacing",
            HeightRequest = 400,
            BackgroundColor = Colors.LightGray,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
            {
                ItemSpacing = 10,
                SnapPointsType = SnapPointsType.MandatorySingle,
            },
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

        carouselView.CurrentItemChanged += OnCurrentItemChanged;

        currentItemLabel = new Label
        {
            AutomationId = "Issue32048StatusLabel",
            Text = "Failure"
        };

        Grid grid = new Grid
        {
            Padding = 25,
            RowSpacing = 10,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            }
        };

        grid.Add(carouselView);
        grid.Add(currentItemLabel, row: 1);

        Content = grid;
    }

    void OnCurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
    {
        if (e.CurrentItem is not null && e.CurrentItem.ToString() != firstItem)
        {
            currentItemLabel.Text = "Success";
        }
    }
}
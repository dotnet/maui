using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 22507, "CarouselView behaves strangely when swiping vertically in view", PlatformAffected.Android)]
public class Issue22507 : ContentPage
{
    public ObservableCollection<Issue22507Model> ItemsList { get; set; }

    public Issue22507()
    {
        ItemsList = new ObservableCollection<Issue22507Model>
        {
            new Issue22507Model(
            "Page 1",
             Enumerable.Range('A', 20)
             .Select(c => $"Item {(char)c}")
             .ToArray()),

            new Issue22507Model(
            "Page 2",
            Enumerable.Range(1, 20)
            .Select(i => $"Item {i}")
            .ToArray())
        };
        Grid mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            }
        };
        // Create CarouselView
        var mainCarousel = new CarouselView
        {
            BackgroundColor = Colors.Yellow,
            Margin = new Thickness(10),
            IsBounceEnabled = true,
            IsSwipeEnabled = true,
            Loop = false,
            ItemsSource = ItemsList,
            ItemTemplate = new DataTemplate(() =>
            {
                // Grid for each carousel item
                var grid = new Grid
                {
                    BackgroundColor = Colors.White,
                    Padding = 10,
                    RowDefinitions =
                    {
                            new RowDefinition { Height = GridLength.Auto },
                            new RowDefinition { Height = GridLength.Star }
                    }
                };

                // Label for Title
                var titleLabel = new Label
                {
                    FontSize = 18,
                    TextColor = Colors.Black,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                titleLabel.SetBinding(Label.TextProperty, "Title");

                // CollectionView for nested items
                var collectionView = new CollectionView
                {
                    BackgroundColor = Colors.LightBlue,
                    AutomationId = "Issue22507CollectionView",
                    ItemTemplate = new DataTemplate(() =>
                    {
                        var itemLabel = new Label
                        {
                            FontSize = 16,
                            Padding = 10
                        };
                        itemLabel.SetBinding(Label.TextProperty, ".");
                        return itemLabel;
                    })
                };
                collectionView.SetBinding(CollectionView.ItemsSourceProperty, "Items");

                grid.Add(titleLabel);
                grid.Add(collectionView, 0, 1);

                return grid;
            })
        };
        Label label = new Label
        {
            Text = "Swipe vertically on the items below. CarouselView should not interfere with vertical scrolling.",
            AutomationId = "Issue22507Label"
        };
        mainGrid.Add(label);
        Grid.SetRow(label, 0);
        mainGrid.Add(mainCarousel);
        Grid.SetRow(mainCarousel, 1);

        Content = mainGrid;
    }
}

public class Issue22507Model
{
    public string Title { get; set; }
    public ObservableCollection<string> Items { get; set; }

    public Issue22507Model(string title, string[] items)
    {
        Title = title;
        Items = new ObservableCollection<string>(items);
    }
}
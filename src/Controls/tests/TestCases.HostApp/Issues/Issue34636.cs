using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34636, "CollectionView ItemSpacing - First and last item on the list is truncated after changing Spacing value", PlatformAffected.Android)]
public class Issue34636 : ContentPage
{
    static readonly IReadOnlyList<MonkeyItem> MonkeyData = new[]
    {
        new MonkeyItem("Baboon", Colors.Red, "Africa & Asia"),
        new MonkeyItem("Capuchin Monkey", Colors.Blue, "Central & South America"),
        new MonkeyItem("Blue Monkey", Colors.Green, "Central and East Africa"),
        new MonkeyItem("Squirrel Monkey", Colors.Orange, "Central & South America"),
        new MonkeyItem("Golden Lion Tamarin", Colors.Purple, "Brazil"),
        new MonkeyItem("Howler Monkey", Colors.Brown, "South America"),
        new MonkeyItem("Japanese Macaque", Colors.Teal, "Japan"),
        new MonkeyItem("Mandrill", Colors.Indigo, "Southern Cameroon, Gabon, Equatorial Guinea, and Congo"),
        new MonkeyItem("Proboscis Monkey", Colors.Red, "Borneo"),
        new MonkeyItem("Red-shanked Douc", Colors.Blue, "Vietnam, Laos"),
        new MonkeyItem("Gray-shanked Douc", Colors.Green, "Vietnam"),
        new MonkeyItem("Golden Snub-nosed Monkey", Colors.Orange, "China"),
        new MonkeyItem("Black Snub-nosed Monkey", Colors.Purple, "China"),
        new MonkeyItem("Tonkin Snub-nosed Monkey", Colors.Brown, "Vietnam"),
        new MonkeyItem("Thomas's Langur", Colors.Teal, "Indonesia"),
        new MonkeyItem("Purple-faced Langur", Colors.Indigo, "Sri Lanka"),
    };

    readonly ObservableCollection<MonkeyItem> _items = new(MonkeyData);
    readonly LinearItemsLayout _itemsLayout = new(ItemsLayoutOrientation.Vertical) { ItemSpacing = 0 };
    CollectionView2 _collectionView;

    public Issue34636()
    {
        Title = "CollectionView ItemSpacing regression";

        _collectionView = new CollectionView2
        {
            AutomationId = "MonkeyCollectionView",
            ItemsSource = _items,
            ItemsLayout = _itemsLayout,
            ItemTemplate = new DataTemplate(() =>
            {
                var boxView = new BoxView
                {
                    HeightRequest = 60,
                    WidthRequest = 60
                };
                boxView.SetBinding(BoxView.ColorProperty, nameof(MonkeyItem.Color));
                Grid.SetRowSpan(boxView, 2);

                var nameLabel = new Label
                {
                    FontAttributes = FontAttributes.Bold
                };
                nameLabel.SetBinding(Label.TextProperty, nameof(MonkeyItem.Name));
                Grid.SetColumn(nameLabel, 1);

                var locationLabel = new Label
                {
                    FontAttributes = FontAttributes.Italic,
                    VerticalOptions = LayoutOptions.End
                };
                locationLabel.SetBinding(Label.TextProperty, nameof(MonkeyItem.Location));
                Grid.SetRow(locationLabel, 1);
                Grid.SetColumn(locationLabel, 1);

                return new Grid
                {
                    Padding = new Thickness(10),
                    RowDefinitions =
                    {
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Auto }
                    },
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = GridLength.Star }
                    },
                    Children = { boxView, nameLabel, locationLabel }
                };
            })
        };

        var instructions = new StackLayout
        {
            Children =
            {
                new Label { Text = "1. The Monkeys are displayed in a single column list." },
                new Label { Text = "2. The test passes if the spacing between the Monkeys changes vertically according to the set value." }
            }
        };

        var updateButton = new Button
        {
            AutomationId = "ChangeSpacingButton",
            Text = "Update"
        };
        updateButton.Clicked += (_, _) =>
        {
            _itemsLayout.ItemSpacing = 70;
        };

        Content = new Grid
        {
            Margin = new Thickness(20),
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            },
            Children =
            {
                instructions,
                updateButton,
                _collectionView
            }
        };

        Grid.SetRow(instructions, 0);
        Grid.SetRow(updateButton, 1);
        Grid.SetRow(_collectionView, 2);
    }

    class MonkeyItem
    {
        public string Name { get; set; }
        public Color Color { get; set; }
        public string Location { get; set; }

        public MonkeyItem(string name, Color color, string location)
        {
            Name = name;
            Color = color;
            Location = location;
        }
    }
}

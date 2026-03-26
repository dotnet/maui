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
    };

    readonly ObservableCollection<MonkeyItem> _items = new(MonkeyData);
    readonly LinearItemsLayout _itemsLayout = new(ItemsLayoutOrientation.Vertical) { ItemSpacing = 0 };
    CollectionView _collectionView;

    public Issue34636()
    {
        Title = "CollectionView ItemSpacing regression";

        _collectionView = new CollectionView
        {
            AutomationId = "MonkeyCollectionView",
            BackgroundColor = Colors.LightGray,
            VerticalOptions = LayoutOptions.Fill,
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

        var changeSpacingButton = new Button
        {
            AutomationId = "ChangeSpacingButton",
            Text = "Change spacing to 70"
        };
        changeSpacingButton.Clicked += (_, _) =>
        {
            _itemsLayout.ItemSpacing = 70;
            changeSpacingButton.Text = "Spacing changed to 70";
        };

        Content = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            },
            Children =
            {
                changeSpacingButton,
                _collectionView
            }
        };

        Grid.SetRow(changeSpacingButton, 0);
        Grid.SetRow(_collectionView, 1);
    }

    record MonkeyItem(string Name, Color Color, string Location);
}

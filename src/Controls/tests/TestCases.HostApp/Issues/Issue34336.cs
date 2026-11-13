using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

// Reproduces: [iOS] CollectionView has excessive height if ObservableCollection source delayed in loading
// A horizontal CollectionView inside a Grid row set to Auto should resize to fit its content
// even when the ItemsSource is populated after a delay (after the page has already appeared).
[Issue(IssueTracker.Github, 34336,
    "[iOS] CollectionView has excessive height if ObservableCollection source delayed in loading",
    PlatformAffected.iOS)]
public class Issue34336 : ContentPage
{
    readonly ObservableCollection<Issue34336Item> _items = [];

    public Issue34336()
    {
        // Inline DataTemplate mirroring the ItemCard ContentView from the repro repo
        var itemTemplate = new DataTemplate(() =>
        {
            var nameLabel = new Label { FontSize = 14, TextTransform = TextTransform.Uppercase };
            nameLabel.SetBinding(Label.TextProperty, "Name");
            // AutomationId is bound to Name so the test can reliably wait for "ITEM 0" on all platforms
            nameLabel.SetBinding(Label.AutomationIdProperty, "Name");

            var descLabel = new Label { LineBreakMode = LineBreakMode.WordWrap };
            descLabel.SetBinding(Label.TextProperty, "Description");

            var textStack = new VerticalStackLayout { Spacing = 2 };
            textStack.Children.Add(nameLabel);
            textStack.Children.Add(descLabel);
            Grid.SetColumn(textStack, 1);

            var cardGrid = new Grid { VerticalOptions = LayoutOptions.Start };
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition(150));
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            cardGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            cardGrid.Children.Add(new BoxView { Color = Colors.LightGreen, HeightRequest = 150, WidthRequest = 150 });
            cardGrid.Children.Add(textStack);

            return new Border
            {
                BackgroundColor = Colors.DarkGray,
                StrokeShape = new RoundRectangle { CornerRadius = 5 },
                MinimumHeightRequest = 250,
                Content = cardGrid
            };
        });

        // CollectionView inside a Grid row set to Auto — mirrors the repro project layout
        var collectionView = new CollectionView
        {
            AutomationId = "ItemCollection",
            MinimumHeightRequest = 200,
            ItemsSource = _items,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal) { ItemSpacing = 7.5 },
            ItemTemplate = itemTemplate
        };

        var innerGrid = new Grid
        {
            BackgroundColor = Colors.Gray
        };
        innerGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        innerGrid.Children.Add(collectionView);

        // Label positioned directly below the CollectionView's grid row.
        // Used by the test to measure that the CollectionView did not take up excessive space.
        var belowLabel = new Label
        {
            AutomationId = "BelowCollectionViewLabel",
            Text = "Below CollectionView",
            BackgroundColor = Colors.LightBlue,
            HorizontalTextAlignment = TextAlignment.Center
        };

        var outerGrid = new Grid();
        outerGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        outerGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        outerGrid.Children.Add(innerGrid);
        Grid.SetRow(belowLabel, 1);
        outerGrid.Children.Add(belowLabel);

        Content = outerGrid;

        _ = LoadDataAsync();
    }

    async Task LoadDataAsync()
    {
        // 1-second delay to replicate the original DataService.LoadData() in the repro repo.
        // This is the trigger for the bug: items arrive after the page has already appeared.
        await Task.Delay(1000);

        for (int i = 0; i < 10; i++)
        {
            _items.Add(new Issue34336Item(
                $"ITEM {i}",
                $"This is the description for item {i}."));
        }
    }
}

record Issue34336Item(string Name, string Description);

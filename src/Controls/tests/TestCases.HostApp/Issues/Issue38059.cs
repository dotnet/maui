namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38059, "CollectionView VerticalGrid has excessive spacing without an ItemTemplate", PlatformAffected.UWP)]
public class Issue38059 : ContentPage
{
    public Issue38059()
    {
        var instructions = new Label
        {
            AutomationId = "InstructionsLabel",
            Text = "The test passes if the monkey names are displayed in a compact two-column grid."
        };

        var collectionView = new CollectionView
        {
            AutomationId = "MonkeyCollectionView",
            ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
            ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical),
            ItemsSource = new[]
            {
                "Baboon",
                "Capuchin Monkey",
                "Blue Monkey",
                "Squirrel Monkey",
                "Golden Lion Tamarin",
                "Howler Monkey",
                "Japanese Macaque",
                "Mandrill",
                "Proboscis Monkey",
                "Red-shanked Douc",
                "Gray-shanked Douc",
                "Golden Snub-nosed Monkey",
                "Black Snub-nosed Monkey",
                "Tonkin Snub-nosed Monkey",
                "Thomas's Langur",
                "Purple-faced Langur",
                "Gelada"
            }
        };

        var root = new Grid
        {
            Margin = new Thickness(20),
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        root.Add(instructions);
        root.Add(collectionView, row: 1);
        Content = root;
    }
}

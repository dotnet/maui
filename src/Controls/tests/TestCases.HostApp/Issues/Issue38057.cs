namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38057, "CollectionView MakeVisible scrolls the target item to the top", PlatformAffected.UWP)]
public class Issue38057 : ContentPage
{
    const string TargetMonkey = "Proboscis Monkey";

    readonly CollectionView _collectionView;

    public Issue38057()
    {
        string[] monkeyNames =
        [
            "Baboon",
            "Blue Monkey",
            "Capuchin Monkey",
            "Colobus Monkey",
            "Common Marmoset",
            "De Brazza's Monkey",
            "Dusky Leaf Monkey",
            "Golden Lion Tamarin",
            "Green Monkey",
            "Howler Monkey",
            "Japanese Macaque",
            "Mandrill",
            "Mona Monkey",
            "Patas Monkey",
            "Pied Tamarin",
            "Pygmy Marmoset",
            "Red-shanked Douc",
            "Rhesus Macaque",
            "Saki Monkey",
            "Siamang",
            "Squirrel Monkey",
            "Tibetan Macaque",
            "Vervet Monkey",
            "White-faced Capuchin",
            TargetMonkey,
            "Black Snub-nosed Monkey",
            "Cotton-top Tamarin",
            "Gelada",
            "Golden Snub-nosed Monkey",
            "Spider Monkey"
        ];

        _collectionView = new CollectionView
        {
            AutomationId = "MonkeyCollectionView",
            ItemsSource = monkeyNames,
            ItemTemplate = new DataTemplate(() =>
            {
                var label = new Label
                {
                    HeightRequest = 48,
                    Padding = new Thickness(12),
                    VerticalTextAlignment = TextAlignment.Center
                };

                label.SetBinding(Label.TextProperty, ".");
                label.SetBinding(Label.AutomationIdProperty, ".");
                return label;
            })
        };

        var scrollButton = new Button
        {
            AutomationId = "ScrollToProboscisMonkeyButton",
            Text = "Scroll to Proboscis Monkey",
            Command = new Command(() => _collectionView.ScrollTo(TargetMonkey, position: ScrollToPosition.MakeVisible, animate: false))
        };

        var layout = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            }
        };

        layout.Add(scrollButton);
        layout.Add(_collectionView, 0, 1);
        Content = layout;
    }
}

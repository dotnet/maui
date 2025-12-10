namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 15778, "CollectionView SelectionChanged gets fired when performing swipe using swipe view", PlatformAffected.Android)]
public class Issue15778 : ContentPage
{
    private Label statusLabel;

    public Issue15778()
    {
        Title = "SwipeView in CollectionView";

        statusLabel = new Label
        {
            Text = "SelectionChanged Not Triggered",
            Padding = new Thickness(10),
            AutomationId = "StatusLabel"
        };

        var collectionView = new CollectionView
        {
            SelectionMode = SelectionMode.Single,
            AutomationId = "TestCollectionView",
            ItemTemplate = new DataTemplate(() =>
            {
                var swipeView = new SwipeView
                {
                    LeftItems = new SwipeItems
                    {
                        new SwipeItem
                        {
                            Text = "Delete",
                            BackgroundColor = Colors.Red,
                            AutomationId = "DeleteSwipeItem"
                        }
                    }
                };

                var label = new Label { Padding = new Thickness(10) };
                label.SetBinding(Label.TextProperty, ".");

                swipeView.Content = new VerticalStackLayout
                {
                    BackgroundColor = Colors.LightGray,
                    Padding = new Thickness(5),
                    Children = { label }
                };

                return swipeView;
            }),
            ItemsSource = new List<string>
            {
                "Item 1",
                "Item 2",
                "Item 3"
            }
        };
        collectionView.SelectionChanged += OnSelectionChanged;

        Content = new VerticalStackLayout
        {
            Padding = new Thickness(10),
            Spacing = 10,
            Children = { statusLabel, collectionView }
        };
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        statusLabel.Text = "SelectionChanged Triggered";
    }
}

using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33614, "CollectionView Scrolled event reports incorrect FirstVisibleItemIndex after programmatic ScrollTo", PlatformAffected.iOS)]
public class Issue33614 : ContentPage
{
    public ObservableCollection<string> Items { get; set; }
    private Label _firstIndexLabel;
    private CollectionView2 _collectionView;
    public Issue33614()
    {
        Items = new ObservableCollection<string>();
        for (int i = 0; i <= 50; i++)
        {
            Items.Add($"Item_{i}");
        }

        _firstIndexLabel = new Label
        {
            AutomationId = "FirstIndexLabel",
            Text = "FirstVisibleItemIndex: 0"
        };

        var scrollToButton = new Button
        {
            AutomationId = "ScrollToButton",
            Text = "ScrollTo Index 15",
            WidthRequest = 150
        };
        scrollToButton.Clicked += OnScrollToButtonClicked;

        _collectionView = new CollectionView2
        {
            AutomationId = "TestCollectionView",
            ItemsSource = Items,
            HeightRequest = 600,
            ItemTemplate = new DataTemplate(() =>
            {
                var label = new Label();
                label.SetBinding(Label.TextProperty, ".");
                return new Border
                {
                    Margin = new Thickness(5),
                    Padding = new Thickness(10),
                    Stroke = Colors.Gray,
                    Content = label
                };
            })
        };

        _collectionView.Scrolled += OnCollectionViewScrolled;

        Content = new StackLayout
        {
            Children =
            {
                _firstIndexLabel,
                 scrollToButton,
                _collectionView
            }
        };
    }

    private void OnCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        _firstIndexLabel.Text = $"FirstVisibleItemIndex: {e.FirstVisibleItemIndex}";
    }

    private void OnScrollToButtonClicked(object sender, EventArgs e)
    {
        _collectionView.ScrollTo(15, position: ScrollToPosition.Start, animate: true);
    }
}
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38275, "CollectionView Label renders truncated after ItemsSource is swapped with MeasureFirstItem", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue38275 : ContentPage
{
    readonly Issue38275ViewModel _viewModel = new();
    readonly Label _statusLabel;
    bool _ascending;

    public Issue38275()
    {
        Title = "Issue 38275";
        Background = Colors.White;

        var swapButton = new Button
        {
            AutomationId = "Issue38275SwapButton",
            Text = "Swap list"
        };
        swapButton.Clicked += OnSwapClicked;

        _statusLabel = new Label
        {
            AutomationId = "Issue38275StatusLabel",
            Text = "Swap count: 0",
            TextColor = Colors.Black
        };

        var collectionView = new CollectionView2
        {
            AutomationId = "Issue38275CollectionView",
            ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem,
            ItemTemplate = new DataTemplate(CreateItemTemplate)
        };
        collectionView.SetBinding(ItemsView.ItemsSourceProperty, nameof(Issue38275ViewModel.Items));

        Content = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            },
            Children =
            {
                swapButton,
                _statusLabel,
                collectionView
            }
        };
        Grid.SetRow(_statusLabel, 1);
        Grid.SetRow(collectionView, 2);

        BindingContext = _viewModel;
    }

    static View CreateItemTemplate()
    {
        var titleLabel = new Label
        {
            TextColor = Colors.Black,
            VerticalOptions = LayoutOptions.Center
        };
        titleLabel.SetBinding(Label.TextProperty, nameof(Issue38275Item.Title));

        var captionLabel = new Label
        {
            FontSize = 10,
            Text = "Last Issued",
            TextColor = Colors.DarkGray
        };

        var dateLabel = new Label
        {
            HorizontalTextAlignment = TextAlignment.End,
            TextColor = Colors.Black
        };
        dateLabel.SetBinding(Label.TextProperty, nameof(Issue38275Item.DateText));
        dateLabel.SetBinding(AutomationIdProperty, new Binding(nameof(Issue38275Item.Index), stringFormat: "Issue38275Date{0}"));

        var dateLayout = new VerticalStackLayout
        {
            HorizontalOptions = LayoutOptions.End,
            Children = { captionLabel, dateLabel }
        };

        var itemLayout = new Grid
        {
            HeightRequest = 80,
            Padding = 10,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(115)
            }
        };
        itemLayout.Add(titleLabel);
        itemLayout.Add(dateLayout, 1);
        return itemLayout;
    }

    void OnSwapClicked(object sender, EventArgs e)
    {
        _ascending = !_ascending;
        _viewModel.Swap(_ascending);
        _statusLabel.Text = $"Swap count: {_viewModel.SwapCount}";
    }
}

sealed class Issue38275ViewModel : BindableObject
{
    IReadOnlyList<Issue38275Item> _items = CreateItems();

    public IReadOnlyList<Issue38275Item> Items
    {
        get => _items;
        private set
        {
            _items = value;
            OnPropertyChanged();
        }
    }

    public int SwapCount { get; private set; }

    public void Swap(bool ascending)
    {
        var items = CreateItems();
        Items = ascending
            ? items.OrderBy(item => item.DateText).ToList()
            : items.OrderByDescending(item => item.DateText).ToList();
        SwapCount++;
    }

    static List<Issue38275Item> CreateItems() =>
    [
        new(0, "Quarterly planning", "30-APR-2026"),
        new(1, "Design review", "15-MAY-2026"),
        new(2, "Release candidate", "28-MAY-2026"),
        new(3, "Documentation", "05-JUN-2026")
    ];
}

sealed record Issue38275Item(int Index, string Title, string DateText);

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36422, "Changing ItemSpacing at runtime shifts CollectionView's ContentOffset, hiding the first item", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue36422 : ContentPage
{
	private readonly Label _spacingLabel;
    private readonly CollectionView _itemsCollectionView;
    private readonly LinearItemsLayout _itemsLayout;
	private IReadOnlyList<string> Items { get; } = Enumerable.Range(1, 50)
		.Select(index => $"Item {index}")
		.ToArray();
	public Issue36422()
	{
		 _spacingLabel = new Label
        {
            Text = "Item spacing: 0"
        };

        var button = new Button
		{
			Text = "Increase Spacing",
			AutomationId = "IncreaseSpacingButton",
		};

		button.Clicked += OnButtonClicked;

        _itemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
        {
            ItemSpacing = 0
        };

        _itemsCollectionView = new CollectionView
        {
            ItemsLayout = _itemsLayout
        };

        _itemsCollectionView.ItemsSource = Items;

        _itemsCollectionView.ItemTemplate = new DataTemplate(() =>
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, ".");

            return new Border
            {
                Padding = 16,
                BackgroundColor = Colors.LightGray,
                StrokeThickness = 0,
                Content = label
            };
        });

        var grid = new Grid
        {
            Padding = 24,
            RowSpacing = 16,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        grid.Add(_spacingLabel, 0, 0);
        grid.Add(button, 0, 1);
        grid.Add(_itemsCollectionView,0,2);

        Content = grid;
	}

	private void OnButtonClicked(object sender, EventArgs e)
    {
        if (_itemsLayout != null)
            _itemsLayout.ItemSpacing = 90;
        if (_spacingLabel != null)
            _spacingLabel.Text = $"Item spacing: 90";
    }
}

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19771, "CollectionView IsEnabled=false allows touch interactions on iOS and Android", PlatformAffected.iOS | PlatformAffected.Android)]
public class Issue19771 : ContentPage
{
	const string DisableButton = "DisableButton";
	const string TestCollectionView = "TestCollectionView";
	const string InteractionCountLabel = "InteractionCountLabel";
	const string StatusLabel = "StatusLabel";

	private int _interactionCount = 0;
	private Label _interactionLabel;
	private Label _statusLabel;
	private CollectionView _collectionView;

	public Issue19771()
	{
		var items = new List<string> { "Item 1", "Item 2", "Item 3", "Item 4", "Item 5" };

		_statusLabel = new Label
		{
			AutomationId = StatusLabel,
			Text = "CollectionView is ENABLED",
			FontSize = 16,
			FontAttributes = FontAttributes.Bold,
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(10)
		};

		_interactionLabel = new Label
		{
			AutomationId = InteractionCountLabel,
			Text = "Interaction Count: 0",
			FontSize = 14,
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(5)
		};

		var disableButton = new Button
		{
			AutomationId = DisableButton,
			Text = "IsEnabled=False",
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(10)
		};
		disableButton.Clicked += (s, e) => SetEnabled(false);

		_collectionView = new CollectionView
		{
			AutomationId = TestCollectionView,
			Background = Colors.AliceBlue,
			ItemsSource = items,
			SelectionMode = SelectionMode.Single,
			HeightRequest = 300,
			ItemTemplate = new DataTemplate(() =>
			{
				var grid = new Grid { Padding = 10 };
				var label = new Label { TextColor = Colors.Black };
				label.SetBinding(Label.TextProperty, ".");
				grid.Children.Add(label);
				return grid;
			})
		};

		_collectionView.SelectionChanged += (s, e) =>
		{
			_interactionCount++;
			_interactionLabel.Text = $"Interaction Count: {_interactionCount}";
		};

		Content = new StackLayout
		{
			Children = 
			{
				_statusLabel,
				_interactionLabel,
				new StackLayout
				{
					Orientation = StackOrientation.Horizontal,
					HorizontalOptions = LayoutOptions.Center,
					Children = { disableButton }
				},
				_collectionView
			}
		};
	}

	private void SetEnabled(bool isEnabled)
	{
		_collectionView.IsEnabled = isEnabled;
		_statusLabel.Text = isEnabled ? "CollectionView is ENABLED" : "CollectionView is DISABLED";
	}
}

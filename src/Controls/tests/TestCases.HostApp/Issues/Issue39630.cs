namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 39630, "Selecting a CollectionView item invokes an ancestor TapGestureRecognizer", PlatformAffected.UWP | PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue39630 : ContentPage
{
	readonly Label _parentTapCountLabel;
	readonly Label _selectionLabel;
	int _parentTapCount;

	public Issue39630()
	{
		_parentTapCountLabel = new Label
		{
			AutomationId = "Issue39630ParentTapCount",
			Text = "Parent Grid taps: 0"
		};

		_selectionLabel = new Label
		{
			AutomationId = "Issue39630Selection",
			Text = "Selected item: none"
		};

		var collectionView = new CollectionView
		{
			AutomationId = "Issue39630CollectionView",
			HeightRequest = 200,
			ItemsSource = new[] { "Tap this item" },
			SelectionMode = SelectionMode.Single,
			ItemTemplate = new DataTemplate(() =>
			{
				var itemLabel = new Label
				{
					AutomationId = "Issue39630FirstItem",
					Padding = 12
				};
				itemLabel.SetBinding(Label.TextProperty, ".");
				return itemLabel;
			})
		};

		collectionView.SelectionChanged += OnSelectionChanged;

		var root = new Grid
		{
			Padding = 20,
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			}
		};

		var parentTapGesture = new TapGestureRecognizer();
		parentTapGesture.Tapped += OnParentTapped;
		root.GestureRecognizers.Add(parentTapGesture);

		root.Add(_parentTapCountLabel, 0, 0);
		root.Add(_selectionLabel, 0, 1);
		root.Add(collectionView, 0, 2);

		Content = root;
	}

	void OnParentTapped(object sender, TappedEventArgs e)
	{
		_parentTapCount++;
		_parentTapCountLabel.Text = $"Parent Grid taps: {_parentTapCount}";
	}

	void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		_selectionLabel.Text = $"Selected item: {e.CurrentSelection[0]}";
	}
}

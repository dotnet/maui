using System.Collections.ObjectModel;
using Maui.Controls.Sample;

namespace Controls.TestCases.Sample.Issues;

[Issue(IssueTracker.Github, 33401, "CollectionView's SelectionChanged isn't fired on iOS when it's inside a grid with TapGestureRecognizer", PlatformAffected.iOS)]
public class Issue33401 : ContentPage
{
	Label _statusLabel;
	ObservableCollection<string> _items;

	public Issue33401()
	{
		_statusLabel = new Label
		{
			Text = "Status: Waiting for interaction...",
			AutomationId = "StatusLabel",
			Margin = new Thickness(10)
		};

		// Create data for CollectionView
		_items = new ObservableCollection<string>();
		for (int i = 1; i <= 20; i++)
		{
			_items.Add($"Item {i}");
		}

		var collectionView = new CollectionView2
		{
			ItemsSource = _items,
			SelectionMode = SelectionMode.Single,
			Margin = new Thickness(10),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Padding = new Thickness(15),
					VerticalOptions = LayoutOptions.Center
				};
				label.SetBinding(Label.TextProperty, ".");
				
				return label;
			})
		};
		collectionView.AutomationId = "TestCollectionView";

		collectionView.SelectionChanged += OnCollectionViewSelectionChanged;

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			},
			BackgroundColor = Colors.LightGray
		};

		TapGestureRecognizer backgroundTapGestureRecognizer = new TapGestureRecognizer();
		backgroundTapGestureRecognizer.Tapped += OnGridTapped;
		grid.GestureRecognizers.Add(backgroundTapGestureRecognizer);

		grid.Add(_statusLabel, 0, 0);
		grid.Add(collectionView, 0, 1);

		Content = grid;
	}

	private void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection.Count > 0)
		{
			_statusLabel.Text = $"Status: Selected {e.CurrentSelection[0]}";
		}
	}

	private void OnGridTapped(object sender, EventArgs e)
	{
		_statusLabel.Text = "Status: Grid tapped!";
	}
}
using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 18389, "Incorrect scroll position when scrolling to an item with header in CollectionView", PlatformAffected.Android)]
public class Issue18389 : ContentPage
{
	ObservableCollection<string> _items;
	CollectionView _collectionView;
	public Issue18389()
	{
		GenerateItems();

		Grid mainGrid = new Grid
		{
			Padding = new Thickness(20),
			RowSpacing = 10,
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		Button scrollToIndexButton = new Button
		{
			AutomationId = "Issue18389_ScrollToBtn",
			Text = "Click me to scroll to index Count - 1!"
		};
		scrollToIndexButton.Clicked += ScrollToLastItem;

		mainGrid.Add(scrollToIndexButton, 0, 0);

		_collectionView = new CollectionView
		{
			ItemsSource = _items,
			ItemTemplate = new DataTemplate(() =>
			{
				Label label = new Label
				{
					FontSize = 40
				};
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(Label.AutomationIdProperty, ".");

				return label;
			}),
			Header = new Label
			{
				Text = "Header",
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				FontSize = 50,
				FontFamily = "Bold"
			},
			Footer = new Label
			{
				Text = "Footer",
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				FontSize = 50,
				FontFamily = "Bold"
			},
		};

		mainGrid.Add(_collectionView, 0, 1);

		Content = mainGrid;
	}

	void GenerateItems()
	{
		_items = new ObservableCollection<string>();

		for (int i = 1; i <= 20; i++)
		{
			_items.Add($"Item {i}");
		}
	}

	void ScrollToLastItem(object sender, EventArgs e)
	{
		_collectionView.ScrollTo(_items.Count - 1);
	}
}
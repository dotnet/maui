using System.Collections.ObjectModel;
using Maui.Controls.Sample.Issues;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 35806, "Android CollectionView KeepScrollOffset stops working after replacing ItemsSource", PlatformAffected.Android)]
public class Issue35806 : TestContentPage
{
	ObservableCollection<string> items;
	CollectionView collectionView;
	int sourceVersion = 1;

	protected override void Init()
	{
		items = CreateItemsSource(sourceVersion);

		collectionView = new CollectionView
		{
			AutomationId = "CollectionView35806",
			ItemsSource = items,
			ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");
				return new Border
				{
					Content = label,
					Padding = 10,
					Margin = new Thickness(5),
					BackgroundColor = Colors.LightGray,
				};
			})
		};

		var replaceSourceButton = new Button
		{
			Text = "Replace ItemsSource",
			AutomationId = "ReplaceSourceButton",
			Command = new Command(() =>
			{
				sourceVersion++;
				items = CreateItemsSource(sourceVersion);
				collectionView.ItemsSource = items;
			})
		};

		var scrollToTopButton = new Button
		{
			Text = "Scroll To Top",
			AutomationId = "ScrollToTopButton",
			Command = new Command(() =>
			{
				collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
			})
		};

		var insertAtTopButton = new Button
		{
			Text = "Insert At Top",
			AutomationId = "InsertAtTopButton",
			Command = new Command(() =>
			{
				items.Insert(0, $"Inserted-{items.Count + 1}");
			})
		};

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			},
			RowSpacing = 5
		};
		grid.Add(replaceSourceButton, 0, 0);
		grid.Add(scrollToTopButton, 0, 1);
		grid.Add(insertAtTopButton, 0, 2);
		grid.Add(collectionView, 0, 3);

		Content = grid;
	}

	static ObservableCollection<string> CreateItemsSource(int version)
	{
		return new ObservableCollection<string>(
			Enumerable.Range(1, 30).Select(i => $"v{version}-Item {i}"));
	}
}

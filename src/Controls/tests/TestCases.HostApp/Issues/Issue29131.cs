using System.Collections.ObjectModel;
using Maui.Controls.Sample.Issues;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 29131, "Android - KeepScrollOffset doesn't not works as expected when new items are added in CollectionView", PlatformAffected.Android)]
public class Issue29131 : TestContentPage
{
	ObservableCollection<string> items;
	CollectionView collectionView;
	int count = 1;

	protected override void Init()
	{
		items = new ObservableCollection<string>(Enumerable.Range(1, 30).Select(i => $"Item {i}"));

		Button keepScrollOffsetButton = CreateButton("KeepScrollOffset", "KeepScrollOffsetButton", OnKeepScrollOffsetClicked);
		Button addButton = CreateButton("Add Item to Top", "AddNewItem", OnAddItemClicked);
		Button scrollButton = CreateButton("Scroll CollectionView", "ScrollButton", OnScrollButtonClicked);

		collectionView = new CollectionView
		{
			AutomationId = "CollectionView",
			ItemsSource = items,
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

		Grid grid = new Grid
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
		grid.Add(keepScrollOffsetButton, 0, 0);
		grid.Add(addButton, 0, 1);
		grid.Add(scrollButton, 0, 2);
		grid.Add(collectionView, 0, 3);

		Content = grid;
	}

	Button CreateButton(string text, string automationId, EventHandler onClick)
	{
		return new Button
		{
			Text = text,
			AutomationId = automationId,
			Command = new Command(_ => onClick(this, EventArgs.Empty))
		};
	}

	void OnKeepScrollOffsetClicked(object sender, EventArgs e)
	{
		collectionView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;
	}

	void OnScrollButtonClicked(object sender, EventArgs e)
	{
		int index = (count % 2 == 0) ? 0 : items.Count - 1;
		var position = (count % 2 == 0) ? ScrollToPosition.Start : ScrollToPosition.End;
		collectionView.ScrollTo(index, position: position, animate: true);
		count++;
	}

	void OnAddItemClicked(object sender, EventArgs e)
	{
		items.Insert(0, $"Item {items.Count + 1}");
	}
}
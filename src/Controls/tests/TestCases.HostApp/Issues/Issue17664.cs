using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 17664, "Incorrect ItemsViewScrolledEventArgs in CollectionView when IsGrouped is set to true", PlatformAffected.iOS | PlatformAffected.Android)]
public class Issue17664 : ContentPage
{
	CollectionView _collectionView;
	Label descriptionLabel;
	ObservableCollection<Issue17664_ItemModelGroup> _groupedItems;

	public Issue17664()
	{
		Button scrollButton = new Button
		{
			AutomationId = "Issue17664ScrollBtn",
			Text = "Scroll to Category C, Item #2"
		};
		scrollButton.Clicked += ScrollButton_Clicked;

		descriptionLabel = new Label
		{
			AutomationId = "Issue17664DescriptionLabel",
			Text = "Use the button above to scroll the CollectionView.",
			FontSize = 14,
			HorizontalOptions = LayoutOptions.Center
		};

		_collectionView = new CollectionView
		{
			IsGrouped = true,
			GroupHeaderTemplate = new DataTemplate(() =>
			{
				Label label = new Label
				{
					FontAttributes = FontAttributes.Bold,
					BackgroundColor = Colors.LightGray,
					Padding = 10
				};

				label.SetBinding(Label.TextProperty, "Name");
				return label;
			}),
			ItemTemplate = new DataTemplate(() =>
			{
				Label textLabel = new Label
				{
					FontAttributes = FontAttributes.Bold,
					Padding = 30
				};

				textLabel.SetBinding(Label.TextProperty, ".");
				return textLabel;
			})
		};

		_collectionView.Scrolled += (s, e) =>
		{
			var flatItems = _groupedItems.SelectMany(group => group).ToList();
			if (e.LastVisibleItemIndex < flatItems.Count)
			{
				descriptionLabel.Text = flatItems[e.LastVisibleItemIndex];
			}
		};

		List<string> categories = new List<string> { "Category A", "Category B", "Category C" };

		_groupedItems = new ObservableCollection<Issue17664_ItemModelGroup>();

		foreach (var category in categories)
		{
			List<string> items = new List<string>();

			for (int i = 0; i < 5; i++)
			{
				items.Add($"{category} item #{i}");
			}

			_groupedItems.Add(new Issue17664_ItemModelGroup(category, items));
		}

		_collectionView.ItemsSource = _groupedItems;

		Grid grid = new Grid
		{
			RowSpacing = 10,
			Padding = 10,
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		grid.Add(scrollButton, 0, 0);
		grid.Add(descriptionLabel, 0, 1);
		grid.Add(_collectionView, 0, 2);

		Content = grid;
	}

	private void ScrollButton_Clicked(object sender, EventArgs e)
	{
		var targetGroup = _groupedItems.FirstOrDefault(group => group.Name == "Category C");
		var targetItem = targetGroup.FirstOrDefault(item => item == "Category C item #2");

		_collectionView.ScrollTo(targetItem, targetGroup, ScrollToPosition.End);
	}
}

public class Issue17664_ItemModelGroup : ObservableCollection<string>
{
	public string Name { get; set; }

	public Issue17664_ItemModelGroup(string name, IEnumerable<string> items) : base(items)
	{
		Name = name;
	}
}
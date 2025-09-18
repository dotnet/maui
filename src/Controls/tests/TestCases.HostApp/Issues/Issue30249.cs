using System.Collections.ObjectModel;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 30249, "Grouped CollectionView does not trigger the Scrolled event for empty groups", PlatformAffected.iOS)]
public class Issue30249 : ContentPage
{
	CollectionView collectionViewWithEmptyGroups;
	ObservableCollection<Issue30249Group> groupedItems = new ObservableCollection<Issue30249Group>();
	Label scrolledEventStatusLabel;

	public Issue30249()
	{
		Label descriptionLabel = new Label
		{
			Text = "Verify CollectionView Scrolled event is triggered for empty groups",
			Margin = new Thickness(10)
		};

		scrolledEventStatusLabel = new Label
		{
			AutomationId = "ScrolledEventStatusLabel",
			Text = "Failure",
			Margin = new Thickness(10)
		};

		collectionViewWithEmptyGroups = new CollectionView
		{
			AutomationId = "CollectionViewWithEmptyGroups",
			IsGrouped = true
		};
		collectionViewWithEmptyGroups.Scrolled += CollectionView_Scrolled;

		collectionViewWithEmptyGroups.GroupHeaderTemplate = new DataTemplate(() =>
		{
			Label label = new Label
			{
				Padding = new Thickness(10),
			};

			label.SetBinding(Label.TextProperty, "Title");
			return label;
		});

		for (int group = 0; group < 20; group++)
		{
			groupedItems.Add(new Issue30249Group($"Group {group}", new List<string>()));
		}

		collectionViewWithEmptyGroups.ItemsSource = groupedItems;

		Grid grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		grid.Add(descriptionLabel, 0, 0);
		grid.Add(scrolledEventStatusLabel, 0, 1);
		grid.Add(collectionViewWithEmptyGroups, 0, 2);

		Content = grid;
	}

	private void CollectionView_Scrolled(object sender, ItemsViewScrolledEventArgs e)
	{
		scrolledEventStatusLabel.Text = "Success";
	}
}

public class Issue30249Group : List<string>
{
	public string Title { get; set; }

	public Issue30249Group(string title, List<string> items) : base(items)
	{
		Title = title;
	}
}
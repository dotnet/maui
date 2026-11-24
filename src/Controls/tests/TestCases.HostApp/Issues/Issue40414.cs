using System.Collections.Generic;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 40414, "CollectionViewHandler2 does not display grouped CollectionView items after ItemsSource is reset", PlatformAffected.UWP)]
public class Issue40414 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Disabled;
		var collectionPage = new Issue40414CollectionPage();
		var homeButton = new Button { Text = "Navigate away", AutomationId = "NavigateAwayButton" };
		homeButton.Clicked += (_, _) => CurrentItem = Items[0];
		collectionPage.AddNavigationButton(homeButton);

		var gridButton = new Button { Text = "Grouped grid", AutomationId = "OpenGridButton" };
		var listButton = new Button { Text = "Grouped list", AutomationId = "OpenListButton" };
		gridButton.Clicked += (_, _) =>
		{
			collectionPage.UseGridLayout(true);
			CurrentItem = Items[1];
		};
		listButton.Clicked += (_, _) =>
		{
			collectionPage.UseGridLayout(false);
			CurrentItem = Items[1];
		};

		Items.Add(new ShellContent
		{
			Title = "Home",
			Content = new ContentPage
			{
				Content = new VerticalStackLayout { Children = { gridButton, listButton } }
			}
		});
		Items.Add(new ShellContent { Title = "Grouped items", Content = collectionPage });
	}
}

class Issue40414CollectionPage : ContentPage
{
	readonly CollectionView _collectionView;
	readonly HorizontalStackLayout _buttons;
	int _generation;
	bool _useGrid = true;

	public Issue40414CollectionPage()
	{
		_collectionView = new CollectionView
		{
			IsGrouped = true,
			ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
			SelectionMode = SelectionMode.None,
			AutomationId = "GroupedCollectionView",
			BackgroundColor = Colors.Yellow,
			ItemsLayout = new GridItemsLayout(5, ItemsLayoutOrientation.Vertical),
			// A layout as the template root is required to reproduce the issue.
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label { TextColor = Colors.Black };
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(AutomationIdProperty, ".");
				return new HorizontalStackLayout { Children = { label } };
			}),
			GroupHeaderTemplate = new DataTemplate(() =>
			{
				var label = new Label { TextColor = Colors.Black };
				label.SetBinding(Label.TextProperty, nameof(Issue40414Group.GroupName));
				label.SetBinding(AutomationIdProperty, nameof(Issue40414Group.GroupName));
				return new HorizontalStackLayout { Children = { label } };
			})
		};

		var resetButton = new Button
		{
			Text = "Reset ItemsSource",
			AutomationId = "ResetItemsSourceButton"
		};
		resetButton.Clicked += (_, _) => ResetItemsSource();
		_buttons = new HorizontalStackLayout { Children = { resetButton } };

		var grid = new Grid
		{
			RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) }
		};
		grid.Add(_buttons, 0, 0);
		grid.Add(_collectionView, 0, 1);
		Content = grid;
	}

	public void AddNavigationButton(Button button) => _buttons.Add(button);

	public void UseGridLayout(bool useGrid)
	{
		if (_useGrid == useGrid)
		{
			return;
		}

		_useGrid = useGrid;
		_collectionView.ItemsLayout = useGrid
			? new GridItemsLayout(5, ItemsLayoutOrientation.Vertical)
			: new LinearItemsLayout(ItemsLayoutOrientation.Vertical);
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		ResetItemsSource();
	}

	void ResetItemsSource()
	{
		_generation++;
		_collectionView.ItemsSource = CreateGroups();
	}

	List<Issue40414Group> CreateGroups()
	{
		var prefix = $"Generation{_generation}";
		var groups = new List<Issue40414Group>();
		for (int g = 1; g <= 3; g++)
		{
			var items = new List<string>();
			for (int i = 1; i <= 4; i++)
			{
				items.Add($"{prefix}Item{g}_{i}");
			}

			groups.Add(new Issue40414Group($"{prefix}Group{g}", items));
		}

		return groups;
	}
}

public class Issue40414Group : List<string>
{
	public Issue40414Group(string groupName, List<string> items) : base(items)
	{
		GroupName = groupName;
	}

	public string GroupName { get; }
}

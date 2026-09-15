using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34122,
	"I5_EmptyView_Swap - Continuously turning the Toggle EmptyViews on and off would cause an item from the list to show up",
	PlatformAffected.Android)]
public class Issue34122 : ContentPage
{
	readonly ObservableCollection<string> _items;
	readonly List<string> _allItems;
	readonly View _basicEmptyView;
	readonly View _advancedEmptyView;
	bool _useBasicEmptyView;
	CollectionView _collectionView;

	public Issue34122()
	{
		_allItems = new List<string>
		{
			"Baboon", "Capuchin Monkey", "Blue Monkey", "Squirrel Monkey",
			"Golden Lion Tamarin", "Howler Monkey", "Japanese Macaque",
			"Mandrill", "Proboscis Monkey", "Gelada"
		};

		_items = new ObservableCollection<string>(_allItems);

		// Pre-built EmptyViews reused on each toggle — same objects swapped in/out,
		// matching the original ManualTests resource-dictionary approach.
		_basicEmptyView = new StackLayout
		{
			AutomationId = "BasicEmptyView",
			BackgroundColor = Colors.LightBlue,
			Children =
			{
				new Button
				{
					Text = "No items to display.",
					AutomationId = "BasicEmptyViewButton",
					HorizontalOptions = LayoutOptions.Center,
					FontAttributes = FontAttributes.Bold,
					FontSize = 18
				}
			}
		};

		_advancedEmptyView = new StackLayout
		{
			AutomationId = "AdvancedEmptyView",
			BackgroundColor = Colors.LightYellow,
			Children =
			{
				new Button
				{
					Text = "No results matched your filter.",
					AutomationId = "AdvancedEmptyViewButton",
					HorizontalOptions = LayoutOptions.Center,
					FontAttributes = FontAttributes.Bold,
					FontSize = 18
				},
				new Label
				{
					Text = "Try a broader filter?",
					HorizontalOptions = LayoutOptions.Center,
					FontAttributes = FontAttributes.Italic
				}
			}
		};

		_collectionView = new CollectionView
		{
			AutomationId = "MonkeyCollectionView",
			ItemsSource = _items,
			EmptyView = _advancedEmptyView,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label { FontSize = 16, Margin = new Thickness(10, 5) };
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(Label.AutomationIdProperty, ".");
				return label;
			})
		};

		var filterButton = new Button
		{
			Text = "Apply Filter (Clear All)",
			AutomationId = "FilterButton"
		};
		filterButton.Clicked += OnFilterButtonClicked;

		var toggleButton = new Button
		{
			Text = "Toggle EmptyView",
			AutomationId = "ToggleEmptyViewButton"
		};
		toggleButton.Clicked += OnToggleEmptyViewClicked;

		Content = new Grid
		{
			RowDefinitions =
			[
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star),
			],
			Children =
			{
				new VerticalStackLayout { Children = { filterButton, toggleButton } },
				_collectionView
			}
		};

		Grid.SetRow(_collectionView, 1);
	}

	void OnFilterButtonClicked(object sender, EventArgs e)
	{
		// Remove items one-by-one (matching the original ManualTests FilterCommand),
		// which fires multiple CollectionChanged events and puts RecyclerView into
		// the state that triggers the EmptyView-swap bug.
		foreach (var name in _allItems)
			_items.Remove(name);
	}

	void OnToggleEmptyViewClicked(object sender, EventArgs e)
	{
		_useBasicEmptyView = !_useBasicEmptyView;
		_collectionView.EmptyView = _useBasicEmptyView ? _basicEmptyView : _advancedEmptyView;
	}
}

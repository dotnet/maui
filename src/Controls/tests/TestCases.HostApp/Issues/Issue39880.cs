using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 39880, "CollectionView with CanReorderItems crashes when an item is dragged onto a Footer or GroupFooter", PlatformAffected.Android)]
public class Issue39880 : ContentPage
{
	readonly ObservableCollection<string> _items = new() { "A", "B", "C" };
	readonly ObservableCollection<Issue39880Group> _groups = new()
	{
		new Issue39880Group("Fruits") { "Apple", "Banana" },
		new Issue39880Group("Vegetables") { "Carrot", "Leek" },
	};
	readonly Label _orderLabel;
	readonly Label _groupOrderLabel;

	public Issue39880()
	{
		_orderLabel = new Label { AutomationId = "Order39880" };
		_groupOrderLabel = new Label { AutomationId = "GroupOrder39880" };

		// Ungrouped list with a footer
		var list = new CollectionView
		{
			ItemsSource = _items,
			CanReorderItems = true,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label { Padding = 12 };
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(AutomationIdProperty, new Binding(".", stringFormat: "Item39880{0}"));
				return label;
			}),
			Footer = new Label { Text = "Footer", Padding = 12, AutomationId = "Footer39880" },
		};
		list.ReorderCompleted += (_, _) => UpdateOrder();

		// Grouped list with group footers
		var groupedList = new CollectionView
		{
			ItemsSource = _groups,
			IsGrouped = true,
			CanReorderItems = true,
			CanMixGroups = true,
			GroupHeaderTemplate = new DataTemplate(() =>
			{
				var label = new Label { Padding = 12, FontAttributes = FontAttributes.Bold };
				label.SetBinding(Label.TextProperty, nameof(Issue39880Group.Name));
				return label;
			}),
			GroupFooterTemplate = new DataTemplate(() =>
			{
				var label = new Label { Padding = 12, Text = "Group footer" };
				label.SetBinding(AutomationIdProperty, new Binding(nameof(Issue39880Group.Name), stringFormat: "GroupFooter39880{0}"));
				return label;
			}),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label { Padding = 12 };
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(AutomationIdProperty, new Binding(".", stringFormat: "Item39880{0}"));
				return label;
			}),
		};
		groupedList.ReorderCompleted += (_, _) => UpdateOrder();

		var grid = new Grid
		{
			RowDefinitions = new RowDefinitionCollection
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star },
				new RowDefinition { Height = GridLength.Star },
			},
		};
		grid.Add(_orderLabel, 0, 0);
		grid.Add(_groupOrderLabel, 0, 1);
		grid.Add(list, 0, 2);
		grid.Add(groupedList, 0, 3);
		Content = grid;
		UpdateOrder();
	}

	void UpdateOrder()
	{
		_orderLabel.Text = string.Join(",", _items);
		_groupOrderLabel.Text = string.Join("|", _groups.Select(g => $"{g.Name}:{string.Join(",", g)}"));
	}
}

public class Issue39880Group : ObservableCollection<string>
{
	public Issue39880Group(string name) => Name = name;

	public string Name { get; }
}

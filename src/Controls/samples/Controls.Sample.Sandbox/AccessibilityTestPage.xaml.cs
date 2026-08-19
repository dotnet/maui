using System.Collections.ObjectModel;

namespace Maui.Controls.Sample;

public partial class AccessibilityTestPage : ContentPage
{
	int _nextNumber = 11;
	string _statusText = "Select an item to test selection announcements";

	public ObservableCollection<AccessibilityTestItem> Items { get; } = [];
	public ObservableCollection<AccessibilityTestGroup> Groups { get; } = [];

	public IReadOnlyList<string> ScenarioNames { get; } =
	[
		"Vertical list",
		"Horizontal list",
		"Vertical grid",
		"Horizontal grid",
		"Vertical grouping",
		"Horizontal grouping"
	];

	public string StatusText
	{
		get => _statusText;
		private set
		{
			if (_statusText == value)
				return;
			_statusText = value;
			OnPropertyChanged(nameof(StatusText));
		}
	}

	public AccessibilityTestPage()
	{
		InitializeComponent();
		BindingContext = this;
		for (var index = 1; index <= 10; index++)
			Items.Add(CreateItem(index));

		RebuildGroups();
		ScenarioPicker.SelectedIndex = 0;
	}

	void OnScenarioChanged(object? sender, EventArgs e)
	{
		if (ScenarioPicker.SelectedIndex < 0)
			return;

		AccessibilityCollection.SelectedItem = null;
		AccessibilityCollection.IsGrouped = false;
		AccessibilityCollection.ItemsSource = Items;

		switch (ScenarioPicker.SelectedIndex)
		{
			case 0:
				AccessibilityCollection.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);
				break;
			case 1:
				AccessibilityCollection.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);
				break;
			case 2:
				AccessibilityCollection.ItemsLayout = CreateGridLayout(ItemsLayoutOrientation.Vertical);
				break;
			case 3:
				AccessibilityCollection.ItemsLayout = CreateGridLayout(ItemsLayoutOrientation.Horizontal);
				break;
			case 4:
				ConfigureGrouping(ItemsLayoutOrientation.Vertical);
				break;
			case 5:
				ConfigureGrouping(ItemsLayoutOrientation.Horizontal);
				break;
		}

		UpdateStatus($"{ScenarioPicker.SelectedItem}; verify item names and positions");
	}

	void OnAddClicked(object? sender, EventArgs e)
	{
		var item = CreateItem(_nextNumber++);
		Items.Add(item);
		RebuildGroups();
		RefreshGroupedSource();
		UpdateStatus($"Added {item.VisibleText}");
	}

	void OnRemoveClicked(object? sender, EventArgs e)
	{
		if (AccessibilityCollection.SelectedItem is not AccessibilityTestItem item)
			return;
		Items.Remove(item);
		RebuildGroups();
		RefreshGroupedSource();
		UpdateStatus($"Removed {item.VisibleText}");
	}

	void OnMoveClicked(object? sender, EventArgs e)
	{
		if (AccessibilityCollection.SelectedItem is not AccessibilityTestItem item)
			return;
		var index = Items.IndexOf(item);
		var newIndex = (index + 1) % Items.Count;
		Items.Move(index, newIndex);
		RebuildGroups();
		RefreshGroupedSource();
		UpdateStatus($"Moved {item.VisibleText} to position {newIndex + 1}");
	}

	void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (AccessibilityCollection.SelectedItem is AccessibilityTestItem item)
			UpdateStatus($"Selected {item.VisibleText}");
	}

	static GridItemsLayout CreateGridLayout(ItemsLayoutOrientation orientation) => new(3, orientation)
	{
		HorizontalItemSpacing = 8,
		VerticalItemSpacing = 8
	};

	void ConfigureGrouping(ItemsLayoutOrientation orientation)
	{
		AccessibilityCollection.IsGrouped = true;
		AccessibilityCollection.ItemsSource = Groups;
		AccessibilityCollection.ItemsLayout = new LinearItemsLayout(orientation);
	}

	void RebuildGroups()
	{
		Groups.Clear();
		const int groupSize = 5;
		for (var start = 0; start < Items.Count; start += groupSize)
		{
			var group = new AccessibilityTestGroup($"Group {(start / groupSize) + 1}");
			foreach (var item in Items.Skip(start).Take(groupSize))
				group.Add(item);
			Groups.Add(group);
		}
	}

	void RefreshGroupedSource()
	{
		if (!AccessibilityCollection.IsGrouped)
			return;

		AccessibilityCollection.ItemsSource = null;
		AccessibilityCollection.ItemsSource = Groups;
	}

	void UpdateStatus(string action) =>
		StatusText = $"{ScenarioPicker.SelectedItem}: {action}; verify set size is {Items.Count}";

	static AccessibilityTestItem CreateItem(int number) =>
		new($"Item {number}", $"Accessibility item {number}");
}

public sealed record AccessibilityTestItem(string VisibleText, string AccessibleName);

public sealed class AccessibilityTestGroup(string name) : ObservableCollection<AccessibilityTestItem>
{
	public string Name { get; } = name;
}
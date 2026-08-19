using System.Collections.ObjectModel;

namespace Maui.Controls.Sample;

public partial class TabNavigationTestPage : ContentPage
{
	readonly ObservableCollection<TabTestItem> _items = [];
	readonly ObservableCollection<TabTestGroup> _groups = [];
	string _statusText = string.Empty;

	public IReadOnlyList<string> ScenarioNames { get; } =
	[
		"Vertical linear",
		"Horizontal linear",
		"Vertical grid (dynamic span)",
		"Horizontal grid",
		"Vertical grouping",
		"Selection",
		"Item sizing strategy",
		"Horizontal grouping + sizing",
		"Interactive item template"
	];

	public IReadOnlyList<string> SizingStrategies { get; } =
	[
		nameof(ItemSizingStrategy.MeasureAllItems),
		nameof(ItemSizingStrategy.MeasureFirstItem)
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

	public TabNavigationTestPage()
	{
		InitializeComponent();
		BindingContext = this;
		CreateData();
		ScenarioPicker.SelectedIndex = 0;
		SizingPicker.SelectedIndex = 0;
	}

	void CreateData()
	{
		var colors = new[] { Colors.CornflowerBlue, Colors.SeaGreen, Colors.IndianRed, Colors.DarkGoldenrod };
		for (var index = 1; index <= 24; index++)
			_items.Add(new TabTestItem(index, $"Item {index}", colors[(index - 1) % colors.Length]));

		for (var groupIndex = 0; groupIndex < 4; groupIndex++)
		{
			var group = new TabTestGroup($"Group {groupIndex + 1}");
			foreach (var item in _items.Skip(groupIndex * 6).Take(6))
				group.Add(item);
			_groups.Add(group);
		}
	}

	void OnScenarioChanged(object? sender, EventArgs e) => ApplyScenario();

	void OnSizingChanged(object? sender, EventArgs e)
	{
		TestCollection.ItemSizingStrategy = SizingPicker.SelectedIndex == 1
			? ItemSizingStrategy.MeasureFirstItem
			: ItemSizingStrategy.MeasureAllItems;
		UpdateStatus();
	}

	void OnSpanChanged(object? sender, ValueChangedEventArgs e)
	{
		if (TestCollection.ItemsLayout is GridItemsLayout gridLayout)
			gridLayout.Span = (int)e.NewValue;
		UpdateStatus();
	}

	void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) => UpdateStatus();

	void OnResetClicked(object? sender, EventArgs e)
	{
		SpanStepper.Value = 2;
		SizingPicker.SelectedIndex = 0;
		TestCollection.SelectedItem = null;
		ApplyScenario();
	}

	void ApplyScenario()
	{
		if (ScenarioPicker.SelectedIndex < 0)
			return;

		TestCollection.SelectedItem = null;
		TestCollection.SelectionMode = SelectionMode.None;
		TestCollection.IsGrouped = false;
		TestCollection.ItemsSource = _items;
		TestCollection.ItemTemplate = (DataTemplate)Resources["StandardItemTemplate"];
		TestCollection.ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems;
		SpanStepper.IsEnabled = false;
		SizingPicker.IsEnabled = false;

		switch (ScenarioPicker.SelectedIndex)
		{
			case 0: TestCollection.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical); break;
			case 1: TestCollection.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal); break;
			case 2:
				SpanStepper.IsEnabled = true;
				TestCollection.ItemsLayout = CreateGridLayout(ItemsLayoutOrientation.Vertical);
				break;
			case 3:
				SpanStepper.IsEnabled = true;
				TestCollection.ItemsLayout = CreateGridLayout(ItemsLayoutOrientation.Horizontal);
				break;
			case 4: ConfigureGrouping(ItemsLayoutOrientation.Vertical); break;
			case 5:
				TestCollection.SelectionMode = SelectionMode.Single;
				TestCollection.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);
				break;
			case 6:
				SizingPicker.IsEnabled = true;
				TestCollection.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);
				OnSizingChanged(this, EventArgs.Empty);
				break;
			case 7:
				SizingPicker.IsEnabled = true;
				ConfigureGrouping(ItemsLayoutOrientation.Horizontal);
				OnSizingChanged(this, EventArgs.Empty);
				break;
			case 8:
				TestCollection.ItemTemplate = (DataTemplate)Resources["InteractiveItemTemplate"];
				TestCollection.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);
				break;
		}

		UpdateStatus();
	}

	GridItemsLayout CreateGridLayout(ItemsLayoutOrientation orientation) => new((int)SpanStepper.Value, orientation)
	{
		HorizontalItemSpacing = 6,
		VerticalItemSpacing = 6
	};

	void ConfigureGrouping(ItemsLayoutOrientation orientation)
	{
		TestCollection.IsGrouped = true;
		TestCollection.ItemsSource = _groups;
		TestCollection.ItemsLayout = new LinearItemsLayout(orientation);
	}

	void UpdateStatus()
	{
		var span = TestCollection.ItemsLayout is GridItemsLayout grid ? $", span {grid.Span}" : string.Empty;
		var selected = TestCollection.SelectedItem is TabTestItem item ? $", selected {item.Name}" : string.Empty;
		StatusText = $"{ScenarioPicker.SelectedItem ?? "Scenario"}{span}, {TestCollection.ItemSizingStrategy}{selected}";
	}
}

public sealed record TabTestItem(int Number, string Name, Color Color);

public sealed class TabTestGroup(string name) : ObservableCollection<TabTestItem>
{
	public string Name { get; } = name;
}
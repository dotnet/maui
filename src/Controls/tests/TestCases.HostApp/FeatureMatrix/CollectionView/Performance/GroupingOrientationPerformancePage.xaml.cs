using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public partial class GroupingOrientationPerformancePage : ContentPage
{
	private readonly ObservableCollection<PerfItemGroup> _groups = [];
	private readonly Stopwatch _stopwatch = new();
	private readonly Random _random = new();
	private bool _isGrouped = true;
	private ItemsLayoutOrientation _currentOrientation = ItemsLayoutOrientation.Vertical;

	private static readonly Color[] AvatarColors =
	[
		Colors.DodgerBlue, Colors.Coral, Colors.MediumSeaGreen,
		Colors.MediumPurple, Colors.Goldenrod, Colors.Tomato,
		Colors.SlateBlue, Colors.Teal, Colors.OrangeRed, Colors.CadetBlue
	];

	private static readonly string[] Categories =
	[
		"Electronics", "Clothing", "Books", "Sports", "Food",
		"Music", "Travel", "Health", "Gaming", "Art",
		"Science", "Finance", "Education", "Nature", "Pets",
		"Movies", "Cooking", "Fitness", "Tech", "Design"
	];

	public GroupingOrientationPerformancePage()
	{
		InitializeComponent();
		CV.ItemsSource = _groups;
		LoadGroupedItems(100, 5);
	}

	#region Orientation Changes

	private void OnVertical(object? sender, EventArgs e) => ChangeOrientation(ItemsLayoutOrientation.Vertical);
	private void OnHorizontal(object? sender, EventArgs e) => ChangeOrientation(ItemsLayoutOrientation.Horizontal);

	private void ChangeOrientation(ItemsLayoutOrientation orientation)
	{
		_stopwatch.Restart();

		_currentOrientation = orientation;
		CV.ItemsLayout = new LinearItemsLayout(orientation) { ItemSpacing = 2 };

		_stopwatch.Stop();

		var totalItems = _groups.Sum(g => g.Count);
		OrientationLabel.Text = $"Orientation: {orientation}";
		StatusLabel.Text = $"Orientation changed to {orientation} in {_stopwatch.ElapsedMilliseconds}ms";
		TimingLabel.Text = $"Orientation Change: {_stopwatch.ElapsedMilliseconds}ms";

		VerticalBtn.BackgroundColor = orientation == ItemsLayoutOrientation.Vertical ? Colors.DodgerBlue : Colors.LightGray;
		VerticalBtn.TextColor = orientation == ItemsLayoutOrientation.Vertical ? Colors.White : Colors.Black;
		HorizontalBtn.BackgroundColor = orientation == ItemsLayoutOrientation.Horizontal ? Colors.DodgerBlue : Colors.LightGray;
		HorizontalBtn.TextColor = orientation == ItemsLayoutOrientation.Horizontal ? Colors.White : Colors.Black;

		Debug.WriteLine($"[Perf Grouping] Orientation={orientation} in {_stopwatch.ElapsedMilliseconds}ms (items: {totalItems})");
	}

	#endregion

	#region Grouping Toggle

	private void OnGroupingOn(object? sender, EventArgs e) => SetGrouping(true);
	private void OnGroupingOff(object? sender, EventArgs e) => SetGrouping(false);

	private void SetGrouping(bool isGrouped)
	{
		_stopwatch.Restart();

		_isGrouped = isGrouped;
		CV.IsGrouped = isGrouped;

		_stopwatch.Stop();

		GroupOnBtn.BackgroundColor = isGrouped ? Colors.DodgerBlue : Colors.LightGray;
		GroupOnBtn.TextColor = isGrouped ? Colors.White : Colors.Black;
		GroupOffBtn.BackgroundColor = !isGrouped ? Colors.DodgerBlue : Colors.LightGray;
		GroupOffBtn.TextColor = !isGrouped ? Colors.White : Colors.Black;

		StatusLabel.Text = $"Grouping {(isGrouped ? "ON" : "OFF")} in {_stopwatch.ElapsedMilliseconds}ms";
		TimingLabel.Text = $"Grouping Toggle: {_stopwatch.ElapsedMilliseconds}ms";

		Debug.WriteLine($"[Perf Grouping] Grouping={isGrouped} in {_stopwatch.ElapsedMilliseconds}ms");
	}

	#endregion

	#region Load / Clear / Scroll

	private void OnLoad100(object? sender, EventArgs e) => LoadGroupedItems(100, 5);
	private void OnLoad1000(object? sender, EventArgs e) => LoadGroupedItems(1000, 10);
	private void OnLoad5Groups(object? sender, EventArgs e) => LoadGroupedItems(100, 5);
	private void OnLoad20Groups(object? sender, EventArgs e) => LoadGroupedItems(200, 20);

	private void LoadGroupedItems(int totalItems, int groupCount)
	{
		_stopwatch.Restart();

		_groups.Clear();

		var itemsPerGroup = totalItems / groupCount;
		var remainder = totalItems % groupCount;
		int globalIndex = 1;

		for (int g = 0; g < groupCount; g++)
		{
			var category = Categories[g % Categories.Length];
			var group = new PerfItemGroup(category);

			var count = itemsPerGroup + (g < remainder ? 1 : 0);
			for (int i = 0; i < count; i++)
			{
				group.Add(CreateGroupedItem(globalIndex++, category));
			}

			_groups.Add(group);
		}

		_stopwatch.Stop();

		var actualTotal = _groups.Sum(g => g.Count);
		StatusLabel.Text = $"Loaded {actualTotal} items in {groupCount} groups";
		ItemCountLabel.Text = $"Items: {actualTotal} | Groups: {groupCount}";
		TimingLabel.Text = $"Load Time: {_stopwatch.ElapsedMilliseconds}ms";

		Debug.WriteLine($"[Perf Grouping] Loaded {actualTotal} items in {groupCount} groups in {_stopwatch.ElapsedMilliseconds}ms");
	}

	private PerfGroupedItem CreateGroupedItem(int index, string category)
	{
		var name = GetRandomName();
		return new PerfGroupedItem
		{
			Name = $"{name} #{index}",
			Category = category,
			Index = $"#{index}",
			Initials = $"{name[0]}{name[^1]}".ToUpperInvariant(),
			AvatarColor = AvatarColors[_random.Next(AvatarColors.Length)]
		};
	}

	private string GetRandomName()
	{
		string[] names = ["Alice", "Bob", "Charlie", "Diana", "Edward", "Fiona",
						  "George", "Hannah", "Ivan", "Julia", "Kevin", "Laura",
						  "Michael", "Nancy", "Oscar", "Patricia"];
		return names[_random.Next(names.Length)];
	}

	private void OnClear(object? sender, EventArgs e)
	{
		_stopwatch.Restart();
		_groups.Clear();
		_stopwatch.Stop();

		StatusLabel.Text = $"Cleared in {_stopwatch.ElapsedMilliseconds}ms";
		ItemCountLabel.Text = $"Items: 0 | Groups: 0";
		TimingLabel.Text = $"Clear Time: {_stopwatch.ElapsedMilliseconds}ms";
	}

	private void OnScrollEnd(object? sender, EventArgs e)
	{
		if (_groups.Count == 0 || _groups[^1].Count == 0)
		{
			StatusLabel.Text = "No items to scroll to";
			return;
		}

		_stopwatch.Restart();
		CV.ScrollTo(_groups[^1][^1], _groups[^1], position: ScrollToPosition.End, animate: false);
		_stopwatch.Stop();

		StatusLabel.Text = $"Scrolled to end in {_stopwatch.ElapsedMilliseconds}ms";
	}

	#endregion
}

#region Models

public class PerfGroupedItem
{
	public required string Name { get; set; }
	public required string Category { get; set; }
	public required string Index { get; set; }
	public required string Initials { get; set; }
	public required Color AvatarColor { get; set; }
}

public class PerfItemGroup : ObservableCollection<PerfGroupedItem>
{
	public string Name { get; }

	public PerfItemGroup(string name) : base()
	{
		Name = name;
	}
}

#endregion

using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public partial class GridSpanPerformancePage : ContentPage
{
	private readonly ObservableCollection<GridPerfItem> _items = [];
	private readonly Stopwatch _stopwatch = new();
	private readonly Random _random = new();
	private int _currentSpan = 2;

	private static readonly Color[] AvatarColors =
	[
		Colors.DodgerBlue, Colors.Coral, Colors.MediumSeaGreen,
		Colors.MediumPurple, Colors.Goldenrod, Colors.Tomato,
		Colors.SlateBlue, Colors.Teal
	];

	private static readonly string[] Tags = ["New", "Hot", "Sale", "Featured", "Popular", "Trending"];

	public GridSpanPerformancePage()
	{
		InitializeComponent();
		CV.ItemsSource = _items;
		LoadItems(100);
	}

	#region Span Changes

	private void OnSpan1(object? sender, EventArgs e) => ChangeSpan(1);
	private void OnSpan2(object? sender, EventArgs e) => ChangeSpan(2);
	private void OnSpan3(object? sender, EventArgs e) => ChangeSpan(3);
	private void OnSpan4(object? sender, EventArgs e) => ChangeSpan(4);
	private void OnSpan5(object? sender, EventArgs e) => ChangeSpan(5);

	private void ChangeSpan(int span)
	{
		_stopwatch.Restart();

		_currentSpan = span;
		CV.ItemsLayout = new GridItemsLayout(span, ItemsLayoutOrientation.Vertical)
		{
			HorizontalItemSpacing = 4,
			VerticalItemSpacing = 4
		};

		_stopwatch.Stop();

		SpanLabel.Text = $"Current Span: {span}";
		StatusLabel.Text = $"Span changed to {span} in {_stopwatch.ElapsedMilliseconds}ms";
		TimingLabel.Text = $"Span Change: {_stopwatch.ElapsedMilliseconds}ms";

		Debug.WriteLine($"[Perf GridSpan] Span changed to {span} in {_stopwatch.ElapsedMilliseconds}ms (items: {_items.Count})");
	}

	#endregion

	#region Load / Clear / Scroll

	private void OnLoad100(object? sender, EventArgs e) => LoadItems(100);
	private void OnLoad1000(object? sender, EventArgs e) => LoadItems(1000);

	private void LoadItems(int count)
	{
		_stopwatch.Restart();

		_items.Clear();
		for (int i = 0; i < count; i++)
		{
			_items.Add(CreateGridItem(i + 1));
		}

		_stopwatch.Stop();

		StatusLabel.Text = $"Loaded {count} items (span={_currentSpan})";
		ItemCountLabel.Text = $"Items: {_items.Count}";
		TimingLabel.Text = $"Load Time: {_stopwatch.ElapsedMilliseconds}ms";

		Debug.WriteLine($"[Perf GridSpan] Loaded {count} items in {_stopwatch.ElapsedMilliseconds}ms");
	}

	private GridPerfItem CreateGridItem(int index)
	{
		var name = GetRandomName();
		return new GridPerfItem
		{
			Title = $"{name} #{index}",
			Tag = Tags[_random.Next(Tags.Length)],
			Initials = $"{name[0]}{name[^1]}".ToUpperInvariant(),
			AvatarColor = AvatarColors[_random.Next(AvatarColors.Length)]
		};
	}

	private string GetRandomName()
	{
		string[] names = ["Alice", "Bob", "Charlie", "Diana", "Edward", "Fiona",
						  "George", "Hannah", "Ivan", "Julia", "Kevin", "Laura"];
		return names[_random.Next(names.Length)];
	}

	private void OnClear(object? sender, EventArgs e)
	{
		_stopwatch.Restart();
		_items.Clear();
		_stopwatch.Stop();

		StatusLabel.Text = $"Cleared in {_stopwatch.ElapsedMilliseconds}ms";
		ItemCountLabel.Text = $"Items: {_items.Count}";
		TimingLabel.Text = $"Clear Time: {_stopwatch.ElapsedMilliseconds}ms";
	}

	private void OnScrollEnd(object? sender, EventArgs e)
	{
		if (_items.Count == 0)
		{
			StatusLabel.Text = "No items to scroll to";
			return;
		}

		_stopwatch.Restart();
		CV.ScrollTo(_items[^1], position: ScrollToPosition.End, animate: false);
		_stopwatch.Stop();

		StatusLabel.Text = $"Scrolled to end in {_stopwatch.ElapsedMilliseconds}ms";
	}

	#endregion
}

public class GridPerfItem
{
	public required string Title { get; set; }
	public required string Tag { get; set; }
	public required string Initials { get; set; }
	public required Color AvatarColor { get; set; }
}

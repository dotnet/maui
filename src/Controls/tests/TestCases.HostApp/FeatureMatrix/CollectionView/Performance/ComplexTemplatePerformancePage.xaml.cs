using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public partial class ComplexTemplatePerformancePage : ContentPage
{
	private readonly ObservableCollection<PerformanceComplexItem> _items = [];
	private readonly Stopwatch _stopwatch = new();
	private readonly Random _random = new();

	private static readonly string[] Tags = ["New", "Hot", "Sale", "Featured", "Popular", "Trending", "Limited", "Exclusive"];

	private static readonly Color[] AvatarColors =
	[
		Colors.DodgerBlue, Colors.Coral, Colors.MediumSeaGreen,
		Colors.MediumPurple, Colors.Goldenrod, Colors.Tomato,
		Colors.SlateBlue, Colors.Teal
	];

	public ComplexTemplatePerformancePage()
	{
		InitializeComponent();
		CV.ItemsSource = _items;
		LoadItems(100);
	}

	private void OnLoad100(object? sender, EventArgs e) => LoadItems(100);

	private void OnLoad1000(object? sender, EventArgs e) => LoadItems(1000);

	private void LoadItems(int count)
	{
		_stopwatch.Restart();

		_items.Clear();
		for (int i = 0; i < count; i++)
		{
			_items.Add(CreateItem(i + 1));
		}

		_stopwatch.Stop();

		StatusLabel.Text = $"Loaded {count} complex items in {_stopwatch.ElapsedMilliseconds}ms";
		ItemCountLabel.Text = $"Items: {_items.Count}";
		TimingLabel.Text = $"Load Time: {_stopwatch.ElapsedMilliseconds}ms";

		Debug.WriteLine($"[Perf Complex] Loaded {count} items in {_stopwatch.ElapsedMilliseconds}ms");
	}

	private PerformanceComplexItem CreateItem(int index)
	{
		var firstName = GetRandomName();
		var lastName = GetRandomName();

		return new PerformanceComplexItem
		{
			Title = $"{firstName} {lastName}",
			Subtitle = $"Item #{index}",
			Description = $"This is a detailed description for item {index}. It contains multiple lines of text to test truncation and layout behavior.",
			Tag = Tags[_random.Next(Tags.Length)],
			Initials = $"{firstName[0]}{lastName[0]}",
			AvatarColor = AvatarColors[_random.Next(AvatarColors.Length)]
		};
	}

	private string GetRandomName()
	{
		string[] names = ["Alice", "Bob", "Charlie", "Diana", "Edward", "Fiona", "George", "Hannah",
						  "Ivan", "Julia", "Kevin", "Laura", "Michael", "Nancy", "Oscar", "Patricia"];
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
}

public class PerformanceComplexItem
{
	public required string Title { get; set; }
	public required string Subtitle { get; set; }
	public required string Description { get; set; }
	public required string Tag { get; set; }
	public required string Initials { get; set; }
	public required Color AvatarColor { get; set; }
}

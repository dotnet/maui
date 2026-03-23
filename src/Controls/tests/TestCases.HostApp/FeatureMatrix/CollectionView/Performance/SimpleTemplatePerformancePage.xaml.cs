using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class SimpleTemplatePerformancePage : ContentPage
{
	private readonly ObservableCollection<string> _items = [];
	private readonly Stopwatch _stopwatch = new();

	public SimpleTemplatePerformancePage()
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
			_items.Add($"Item {i + 1}");
		}

		_stopwatch.Stop();

		StatusLabel.Text = $"Loaded {count} items in {_stopwatch.ElapsedMilliseconds}ms";
		ItemCountLabel.Text = $"Items: {_items.Count}";
		TimingLabel.Text = $"Load Time: {_stopwatch.ElapsedMilliseconds}ms";

		Debug.WriteLine($"[Perf Simple] Loaded {count} items in {_stopwatch.ElapsedMilliseconds}ms");
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

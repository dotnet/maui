using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public partial class ScrollingPerformancePage : ContentPage
{
	private readonly ObservableCollection<ScrollPerfItem> _items = [];
	private readonly Stopwatch _stopwatch = new();
	private readonly Random _random = new();

	// FPS tracking
	private int _frameCount;
	private int _droppedFrames;
	private DateTime _lastScrollTime;
	private DateTime _scrollStartTime;
	private double _lastScrollOffset;
	private readonly List<double> _frameTimes = [];
	private bool _isScrollTracking;

	private static readonly Color[] AvatarColors =
	[
		Colors.DodgerBlue, Colors.Coral, Colors.MediumSeaGreen,
		Colors.MediumPurple, Colors.Goldenrod, Colors.Tomato,
		Colors.SlateBlue, Colors.Teal
	];

	public ScrollingPerformancePage()
	{
		InitializeComponent();
		CV.ItemsSource = _items;
		LoadItems(500);
	}

	#region Scroll Event Tracking

	private void OnScrolled(object? sender, ItemsViewScrolledEventArgs e)
	{
		if (!_isScrollTracking)
		{
			_isScrollTracking = true;
			_scrollStartTime = DateTime.Now;
			_frameCount = 0;
			_droppedFrames = 0;
			_frameTimes.Clear();
			_lastScrollTime = DateTime.Now;
		}

		var now = DateTime.Now;
		var delta = (now - _lastScrollTime).TotalMilliseconds;
		_frameCount++;
		_frameTimes.Add(delta);

		if (delta > 32 && _frameCount > 1)
		{
			_droppedFrames++;
		}

		_lastScrollTime = now;
		_lastScrollOffset = e.VerticalOffset;

		if (_frameCount % 10 == 0)
		{
			UpdateScrollStats();
		}
	}

	private void UpdateScrollStats()
	{
		var elapsed = (DateTime.Now - _scrollStartTime).TotalSeconds;
		var avgFps = elapsed > 0 ? _frameCount / elapsed : 0;
		var avgFrameTime = _frameTimes.Count > 0 ? _frameTimes.Average() : 0;
		var maxFrameTime = _frameTimes.Count > 0 ? _frameTimes.Max() : 0;

		FpsLabel.Text = $"Scroll FPS: {avgFps:F1} | Avg Frame: {avgFrameTime:F1}ms | Max: {maxFrameTime:F1}ms";
		FrameCountLabel.Text = $"Frames: {_frameCount} | Dropped: {_droppedFrames}";
		TimingLabel.Text = $"Scroll Duration: {elapsed:F1}s | Offset: {_lastScrollOffset:F0}";

		Debug.WriteLine($"[Perf Scroll] FPS: {avgFps:F1}, Frames: {_frameCount}, Dropped: {_droppedFrames}");
	}

	#endregion

	#region Scroll Actions

	private async void OnSlowScroll(object? sender, EventArgs e)
	{
		if (_items.Count == 0) return;

		ResetScrollStats();
		StatusLabel.Text = "Slow scrolling...";
		_stopwatch.Restart();

		int steps = 40;
		int itemsPerStep = Math.Max(1, _items.Count / steps);

		for (int i = 1; i <= steps; i++)
		{
			int targetIndex = Math.Min(i * itemsPerStep, _items.Count - 1);
			CV.ScrollTo(targetIndex, position: ScrollToPosition.Center, animate: true);
			await Task.Delay(100);
		}

		_stopwatch.Stop();
		_isScrollTracking = false;
		UpdateScrollStats();
		StatusLabel.Text = $"Slow scroll ({steps} steps) in {_stopwatch.ElapsedMilliseconds}ms";
	}

	private async void OnFastScroll(object? sender, EventArgs e)
	{
		if (_items.Count == 0) return;

		ResetScrollStats();
		StatusLabel.Text = "Fast scrolling...";
		_stopwatch.Restart();

		int steps = 20;
		int itemsPerStep = _items.Count / steps;

		for (int i = 1; i <= steps; i++)
		{
			int targetIndex = Math.Min(i * itemsPerStep, _items.Count - 1);
			CV.ScrollTo(targetIndex, position: ScrollToPosition.Center, animate: false);
			await Task.Delay(50);
		}

		for (int i = steps; i >= 0; i--)
		{
			int targetIndex = Math.Max(0, i * itemsPerStep);
			CV.ScrollTo(targetIndex, position: ScrollToPosition.Center, animate: false);
			await Task.Delay(50);
		}

		_stopwatch.Stop();
		_isScrollTracking = false;
		UpdateScrollStats();
		StatusLabel.Text = $"Fast scroll ({steps * 2} jumps) in {_stopwatch.ElapsedMilliseconds}ms";
	}

	private async void OnJumpScroll(object? sender, EventArgs e)
	{
		if (_items.Count == 0) return;

		ResetScrollStats();
		StatusLabel.Text = "Jump scrolling...";
		_stopwatch.Restart();

		for (int i = 0; i < 30; i++)
		{
			int targetIndex = _random.Next(_items.Count);
			CV.ScrollTo(targetIndex, position: ScrollToPosition.Center, animate: false);
			await Task.Delay(30);
		}

		_stopwatch.Stop();
		_isScrollTracking = false;
		UpdateScrollStats();
		StatusLabel.Text = $"Jump scroll (30 random) in {_stopwatch.ElapsedMilliseconds}ms";
	}

	private void OnResetScroll(object? sender, EventArgs e)
	{
		ResetScrollStats();
		CV.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
		StatusLabel.Text = "Reset to top";
	}

	private void ResetScrollStats()
	{
		_frameCount = 0;
		_droppedFrames = 0;
		_frameTimes.Clear();
		_isScrollTracking = false;
		FpsLabel.Text = "Scroll FPS: --";
		FrameCountLabel.Text = "Frames: -- | Dropped: --";
		TimingLabel.Text = "Scroll Time: --";
	}

	#endregion

	#region Load Items

	private void OnLoad100(object? sender, EventArgs e) => LoadItems(100);
	private void OnLoad500(object? sender, EventArgs e) => LoadItems(500);
	private void OnLoad1000(object? sender, EventArgs e) => LoadItems(1000);

	private void LoadItems(int count)
	{
		_stopwatch.Restart();

		_items.Clear();
		for (int i = 0; i < count; i++)
		{
			_items.Add(CreateScrollItem(i + 1));
		}

		_stopwatch.Stop();

		StatusLabel.Text = $"Loaded {count} items in {_stopwatch.ElapsedMilliseconds}ms";
		ItemCountLabel.Text = $"Items: {_items.Count}";
		TimingLabel.Text = $"Load Time: {_stopwatch.ElapsedMilliseconds}ms";
	}

	private ScrollPerfItem CreateScrollItem(int index)
	{
		var name = GetRandomName();
		return new ScrollPerfItem
		{
			Title = $"{name} #{index}",
			Subtitle = $"Scroll performance item {index}",
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

	#endregion
}

public class ScrollPerfItem
{
	public required string Title { get; set; }
	public required string Subtitle { get; set; }
	public required string Initials { get; set; }
	public required Color AvatarColor { get; set; }
}

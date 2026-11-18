using System.Diagnostics;
using System.Globalization;
using Microsoft.Maui.Controls.Animations;

namespace Maui.Controls.Sample;

// The spec's phase 0 device benchmark: 100 elements, each with 3 animated properties, at the display's rate.
// It measures the new engine, then the legacy engine under the same load, and logs PERF| lines. Each frame's
// work is timed from the first element's first write to the last element's last write: the clock ticks runs
// in the order they started, and a run writes its tracks in the order they were added.
public class PerfPage : ContentPage
{
	const int Count = 100;
	const int Columns = 10;
	const int MaxEvents = 64 * 1024;
	static readonly TimeSpan Warmup = TimeSpan.FromSeconds(2);
	static readonly TimeSpan Measurement = TimeSpan.FromSeconds(10);

	readonly BoxView[] _boxes = new BoxView[Count];
	readonly Label _status = new() { FontSize = 13, LineBreakMode = LineBreakMode.WordWrap };
	readonly long[] _times = new long[MaxEvents];
	readonly bool[] _isStart = new bool[MaxEvents];
	readonly List<StoryboardPlayer> _players = new(Count);
	int _events;
	bool _recording;

	public PerfPage()
	{
		var grid = new Grid { RowSpacing = 6, ColumnSpacing = 6, HorizontalOptions = LayoutOptions.Center };
		for (var i = 0; i < Columns; i++)
		{
			grid.RowDefinitions.Add(new RowDefinition(28));
			grid.ColumnDefinitions.Add(new ColumnDefinition(28));
		}

		for (var i = 0; i < Count; i++)
		{
			var box = new BoxView { Color = Color.FromHsla(i / (double)Count, 0.6, 0.5), CornerRadius = 6 };
			grid.Add(box, i % Columns, i / Columns);
			_boxes[i] = box;
		}

		_boxes[0].PropertyChanged += (_, e) => Record(e.PropertyName == nameof(Scale), isStart: true);
		_boxes[Count - 1].PropertyChanged += (_, e) => Record(e.PropertyName == nameof(Opacity), isStart: false);

		Content = new VerticalStackLayout { Spacing = 24, Padding = new Thickness(16, 60, 16, 16), Children = { _status, grid } };
		Loaded += OnLoaded;
	}

	public static bool IsRequested(IActivationState? activationState)
		=> LaunchOptions.Get(activationState, "ANIM_PERF") == "1";

	void Record(bool relevant, bool isStart)
	{
		if (!relevant || !_recording || _events == MaxEvents)
			return;

		_times[_events] = Stopwatch.GetTimestamp();
		_isStart[_events] = isStart;
		_events++;
	}

	async void OnLoaded(object? sender, EventArgs e)
	{
		Loaded -= OnLoaded;
		if (Window is Window window)
		{
			// Lifecycle changes explain gaps in the frames: a stopped window suspends its clock.
			window.Activated += (_, _) => Log("lifecycle|Activated");
			window.Deactivated += (_, _) => Log("lifecycle|Deactivated");
			window.Stopped += (_, _) => Log("lifecycle|Stopped");
			window.Resumed += (_, _) => Log("lifecycle|Resumed");
		}

		await Task.Delay(1500);
		await MeasureAsync("storyboard", StartStoryboards, StopStoryboards);
		await Task.Delay(1500);
		await MeasureAsync("legacy", StartLegacy, StopLegacy);
		Log("done");
	}

	async Task MeasureAsync(string name, Action start, Action stop)
	{
		_status.Text = $"{name}: running {Count} × 3…";
		start();
		await Task.Delay(Warmup);

		var gen0 = GC.CollectionCount(0);
		var gen1 = GC.CollectionCount(1);
		var gen2 = GC.CollectionCount(2);
		var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
		_events = 0;
		_recording = true;
		Log($"{name}|measuring");
		var stopwatch = Stopwatch.StartNew();

		await Task.Delay(Measurement);

		_recording = false;
		var elapsed = stopwatch.Elapsed;
		var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
		var collections = (GC.CollectionCount(0) - gen0, GC.CollectionCount(1) - gen1, GC.CollectionCount(2) - gen2);
		stop();

		var summary = Summarize(name, elapsed, allocated, collections);
		Log(summary);
		_status.Text += Environment.NewLine + summary;
	}

	string Summarize(string name, TimeSpan elapsed, long allocated, (int Gen0, int Gen1, int Gen2) collections)
	{
		var toMs = 1000.0 / Stopwatch.Frequency;
		var intervals = new List<double>();
		var work = new List<double>();
		var gaps = new List<string>();
		long? frameStart = null;
		var matched = false;
		for (var i = 0; i < _events; i++)
		{
			if (_isStart[i])
			{
				if (frameStart is long previous)
				{
					var interval = (_times[i] - previous) * toMs;
					intervals.Add(interval);
					if (interval > 100)
						gaps.Add(string.Create(CultureInfo.InvariantCulture, $"{(previous - _times[0]) * toMs:F0}ms+{interval:F0}ms"));
				}
				frameStart = _times[i];
				matched = false;
			}
			else if (frameStart is long current && !matched)
			{
				work.Add((_times[i] - current) * toMs);
				matched = true;
			}
		}

		var frames = intervals.Count + 1;
		intervals.Sort();
		work.Sort();
		var median = Percentile(intervals, 0.5);
		var dropped = intervals.Count(interval => interval > median * 1.5);
		return string.Create(CultureInfo.InvariantCulture,
			$"{name}|frames={frames}|fps={frames / elapsed.TotalSeconds:F1}" +
			$"|interval_ms p50={median:F2} p90={Percentile(intervals, 0.9):F2} p99={Percentile(intervals, 0.99):F2} max={Percentile(intervals, 1):F2}" +
			$"|late={dropped} ({100.0 * dropped / Math.Max(1, intervals.Count):F1}%)" +
			$"|work_ms p50={Percentile(work, 0.5):F3} p90={Percentile(work, 0.9):F3} p99={Percentile(work, 0.99):F3} max={Percentile(work, 1):F3}" +
			$"|alloc={allocated / (double)frames:F0} B/frame ({allocated / elapsed.TotalSeconds / (1024 * 1024):F2} MB/s)" +
			$"|gc={collections.Gen0}/{collections.Gen1}/{collections.Gen2}" +
			$"|gaps={(gaps.Count == 0 ? "none" : string.Join(",", gaps))}");
	}

	static double Percentile(List<double> sorted, double quantile)
	{
		if (sorted.Count == 0)
			return double.NaN;

		var index = (int)Math.Ceiling(quantile * sorted.Count) - 1;
		return sorted[Math.Clamp(index, 0, sorted.Count - 1)];
	}

	void StartStoryboards()
	{
		var storyboard = Storyboard.Create()
			.WithDuration(TimeSpan.FromSeconds(1))
			.WithEasing(Easing.CubicInOut)
			.WithRepeat(RepeatBehavior.Forever)
			.WithDirection(AnimationDirection.Alternate)
			.AddTrack<double>(ScaleProperty, track => track.At(0, 1.0).At(1, 1.3))
			.AddTrack<double>(RotationProperty, track => track.At(0, 0.0).At(1, 180.0))
			.AddTrack<double>(OpacityProperty, track => track.At(0, 1.0).At(1, 0.4))
			.Build();

		foreach (var box in _boxes)
			_players.Add(storyboard.Begin(box));
	}

	void StopStoryboards()
	{
		foreach (var player in _players)
			player.Stop();
		_players.Clear();
	}

	void StartLegacy()
	{
		foreach (var box in _boxes)
		{
			var animation = new Animation
			{
				{ 0, 1, new Animation(v => box.Scale = v, 1.0, 1.3, Easing.CubicInOut) },
				{ 0, 1, new Animation(v => box.Rotation = v, 0.0, 180.0, Easing.CubicInOut) },
				{ 0, 1, new Animation(v => box.Opacity = v, 1.0, 0.4, Easing.CubicInOut) },
			};
			animation.Commit(box, "Perf", length: 1000, repeat: () => true);
		}
	}

	void StopLegacy()
	{
		foreach (var box in _boxes)
		{
			box.AbortAnimation("Perf");
			box.Scale = 1;
			box.Rotation = 0;
			box.Opacity = 1;
		}
	}

	static void Log(string message) => Console.WriteLine($"PERF|{message}");
}

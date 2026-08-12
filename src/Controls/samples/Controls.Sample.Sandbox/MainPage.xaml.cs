using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	const int RowCount = 8;
	bool _autoRunStarted;
	bool _startupBurstRun;
	bool _startupReported;
	int _latencyProbeSequence;
	bool _latencyProbeRunning;
	string _latencyResult = "Latency pending";
	string _result = string.Empty;
	string _startupResult = "Startup pending";

	public MainPage()
	{
		InitializeComponent();

		Rows = new ObservableCollection<BatchingRowViewModel>(
			Enumerable.Range(1, RowCount).Select(index => new BatchingRowViewModel(index)));
		Mode = HandlerBatchingMetrics.Mode;
		Result = "Ready";
		SandboxStartupMetrics.RegisterFirstFrameSink(OnFirstFrameMeasured);
		BindingContext = this;

		Console.WriteLine($"SANDBOX_HANDLER_BATCHING mode={Mode} rows={RowCount}");
	}

	public ObservableCollection<BatchingRowViewModel> Rows { get; }

	public string Mode { get; }

	public string Result
	{
		get => _result;
		set
		{
			if (_result == value)
				return;

			_result = value;
			OnPropertyChanged();
		}
	}

	public string LatencyResult
	{
		get => _latencyResult;
		set
		{
			if (_latencyResult == value)
				return;

			_latencyResult = value;
			OnPropertyChanged();
		}
	}

	public string StartupResult
	{
		get => _startupResult;
		set
		{
			if (_startupResult == value)
				return;

			_startupResult = value;
			OnPropertyChanged();
		}
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		if (!_startupReported)
		{
			_startupReported = true;
			SandboxStartupMetrics.FirstPageAppeared();
#if STARTUP_UPDATE_BURST
			if (RootLayout.IsLoaded)
				RunStartupUpdateBurstAndScheduleFirstFrame();
			else
				RootLayout.Loaded += OnStartupRootLoaded;
#else
			ScheduleFirstFrameProbe();
#endif
		}

		if (!_autoRunStarted &&
			Environment.GetEnvironmentVariable("HANDLER_BATCHING_AUTORUN") == "1")
		{
			_autoRunStarted = true;
			var repetitions = int.TryParse(
				Environment.GetEnvironmentVariable("HANDLER_BATCHING_AUTORUN_REPETITIONS"),
				out var parsedRepetitions)
				? Math.Max(1, parsedRepetitions)
				: 1;

			Dispatcher.Dispatch(() =>
			{
				for (int repetition = 0; repetition < repetitions; repetition++)
					RunWorkload(100);
			});
		}
	}

	void OnStartupRootLoaded(object? sender, EventArgs e)
	{
		RootLayout.Loaded -= OnStartupRootLoaded;
		RunStartupUpdateBurstAndScheduleFirstFrame();
	}

	void RunStartupUpdateBurstAndScheduleFirstFrame()
	{
		if (_startupBurstRun)
			return;

		_startupBurstRun = true;
		RunStartupUpdateBurst();
		ScheduleFirstFrameProbe();
	}

	void RunStartupUpdateBurst()
	{
		for (int round = 0; round < SandboxStartupConfiguration.UpdateRounds; round++)
		{
			foreach (var row in Rows)
				row.Advance();
		}
	}

	static void ScheduleFirstFrameProbe(Action? callback = null)
	{
		callback ??= SandboxStartupMetrics.FirstFrameReady;
#if ANDROID
		global::Maui.Controls.Sample.Platform.Android.SandboxFirstFrameProbe.Schedule(callback);
#elif IOS || MACCATALYST
		global::Maui.Controls.Sample.Platform.Apple.SandboxFirstFrameProbe.Schedule(callback);
#endif
	}

	void OnFirstFrameMeasured(double elapsedMilliseconds) =>
		StartupResult = FormattableString.Invariant(
			$"first frame: {elapsedMilliseconds:F2} ms");

	void OnSingleRoundClicked(object? sender, EventArgs e) =>
		RunWorkload(1);

	void OnHundredRoundsClicked(object? sender, EventArgs e) =>
		RunWorkload(100);

	void OnLatencyProbeClicked(object? sender, EventArgs e)
	{
		if (_latencyProbeRunning)
			return;

		_latencyProbeRunning = true;
		var sequence = ++_latencyProbeSequence;
		var stopwatch = Stopwatch.StartNew();
		Rows[0].AdvanceLabelOnly();
		ScheduleFirstFrameProbe(() =>
		{
			stopwatch.Stop();
			LatencyResult = FormattableString.Invariant(
				$"latency {sequence}: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");
			_latencyProbeRunning = false;
		});
	}

	void OnResetClicked(object? sender, EventArgs e)
	{
		foreach (var row in Rows)
			row.Reset();

		Result = "Reset";
		Console.WriteLine($"SANDBOX_HANDLER_BATCHING_RESET mode={Mode}");
	}

	void RunWorkload(int rounds)
	{
		HandlerBatchingMetrics.Reset();

		var stopwatch = Stopwatch.StartNew();
		for (int round = 0; round < rounds; round++)
		{
			foreach (var row in Rows)
				row.Advance();
		}
		FlushPendingUpdatesForMeasurement();
		stopwatch.Stop();

		var mapperCalls = HandlerBatchingMetrics.MapperCalls;
		var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
		Result = FormattableString.Invariant(
			$"{rounds} rounds: {mapperCalls} mapper calls in {elapsedMilliseconds:F2} ms");

		Console.WriteLine(
			$"SANDBOX_HANDLER_BATCHING_RESULT mode={Mode} rounds={rounds} rows={RowCount} " +
			FormattableString.Invariant($"mapperCalls={mapperCalls} elapsedMs={elapsedMilliseconds:F4}"));
	}

	void FlushPendingUpdatesForMeasurement()
	{
		foreach (var element in RootLayout.GetVisualTreeDescendants().OfType<VisualElement>())
			_ = element.Handler?.PlatformView;
	}
}

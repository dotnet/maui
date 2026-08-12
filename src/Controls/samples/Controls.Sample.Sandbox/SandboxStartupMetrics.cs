using System.Diagnostics;

namespace Maui.Controls.Sample;

static class SandboxStartupMetrics
{
	static readonly Stopwatch StartupStopwatch = new();
	static double _appBuiltMilliseconds;
	static double _firstPageMilliseconds;
	static int _firstFrameReported;
	static Action<double>? _firstFrameSink;

	public static void BeginStartup()
	{
		_appBuiltMilliseconds = 0;
		_firstPageMilliseconds = 0;
		_firstFrameReported = 0;
		_firstFrameSink = null;
		StartupStopwatch.Restart();
	}

	public static void AppBuilt()
	{
		_appBuiltMilliseconds = StartupStopwatch.Elapsed.TotalMilliseconds;
		Write("app-built", _appBuiltMilliseconds);
	}

	public static void RegisterFirstFrameSink(Action<double> sink) =>
		_firstFrameSink = sink;

	public static void FirstPageAppeared()
	{
		_firstPageMilliseconds = StartupStopwatch.Elapsed.TotalMilliseconds;
		Write("first-page-appeared", _firstPageMilliseconds);
	}

	public static void FirstFrameReady()
	{
		if (Interlocked.Exchange(ref _firstFrameReported, 1) != 0)
			return;

		var firstFrameMilliseconds = StartupStopwatch.Elapsed.TotalMilliseconds;
		Console.WriteLine(FormattableString.Invariant(
			$"SANDBOX_STARTUP_RESULT mode={HandlerBatchingMetrics.Mode} scenario={StartupScenario} appBuiltMs={_appBuiltMilliseconds:F4} firstPageMs={_firstPageMilliseconds:F4} firstFrameMs={firstFrameMilliseconds:F4} mapperCalls={HandlerBatchingMetrics.MapperCalls} measureInvalidations={HandlerBatchingMetrics.MeasureInvalidations} mapperSummary={HandlerBatchingMetrics.MapperCallSummary}"));
		_firstFrameSink?.Invoke(firstFrameMilliseconds);
	}

	static string StartupScenario
		=> SandboxStartupConfiguration.Scenario;

	static void Write(string milestone, double elapsedMilliseconds) =>
		Console.WriteLine(FormattableString.Invariant(
			$"SANDBOX_STARTUP_MILESTONE mode={HandlerBatchingMetrics.Mode} scenario={StartupScenario} milestone={milestone} elapsedMs={elapsedMilliseconds:F4}"));
}

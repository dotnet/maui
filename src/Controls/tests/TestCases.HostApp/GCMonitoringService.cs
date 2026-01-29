#nullable enable
using System.Diagnostics;

namespace Maui.Controls.Sample;

/// <summary>
/// A lightweight service that logs GC activity to Console.WriteLine for test infrastructure.
/// 
/// This service runs automatically on Android and logs GC counts with the [MAUI_GC] prefix.
/// The UITest infrastructure can parse these logs from logcat to track GC activity per test.
/// 
/// Since the app resets between test runs, GC counts are tracked from app startup,
/// giving us a clean per-test baseline.
/// </summary>
public static class GCMonitoringService
{
	private static int _initialGen0Count;
	private static int _initialGen1Count;
	private static int _initialGen2Count;
	private static DateTime _startTime;
	private static bool _isInitialized;
	private static string? _currentTestPage;

	/// <summary>
	/// Initialize the GC monitoring service. Call once at app startup.
	/// </summary>
	public static void Initialize()
	{
		if (_isInitialized)
			return;

		// Force a GC to establish clean baseline
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		_initialGen0Count = GC.CollectionCount(0);
		_initialGen1Count = GC.CollectionCount(1);
		_initialGen2Count = GC.CollectionCount(2);
		_startTime = DateTime.UtcNow;
		_isInitialized = true;

		Log($"Initialized at {_startTime:HH:mm:ss.fff}, baseline Gen0={_initialGen0Count}, Gen1={_initialGen1Count}, Gen2={_initialGen2Count}");
	}

	/// <summary>
	/// Log when a test page is navigated to. Helps correlate GC activity with specific tests.
	/// </summary>
	public static void OnTestPageNavigated(string pageName)
	{
		if (!_isInitialized)
			Initialize();

		_currentTestPage = pageName;
		var (gen0, gen1, gen2) = GetCurrentCounts();
		Log($"TestPage={pageName}, Gen0={gen0}, Gen1={gen1}, Gen2={gen2}");
	}

	/// <summary>
	/// Log when a test page is left. Shows GC activity during the test.
	/// </summary>
	public static void OnTestPageLeft(string pageName)
	{
		if (!_isInitialized)
			return;

		var (gen0, gen1, gen2) = GetCurrentCounts();
		var elapsed = (DateTime.UtcNow - _startTime).TotalSeconds;
		Log($"TestPageLeft={pageName}, Gen0={gen0}, Gen1={gen1}, Gen2={gen2}, ElapsedSec={elapsed:F1}");
		_currentTestPage = null;
	}

	/// <summary>
	/// Log current GC counts. Can be called at any time for debugging.
	/// </summary>
	public static void LogCurrentState(string? context = null)
	{
		if (!_isInitialized)
			Initialize();

		var (gen0, gen1, gen2) = GetCurrentCounts();
		var elapsed = (DateTime.UtcNow - _startTime).TotalSeconds;
		var ctx = context ?? _currentTestPage ?? "Unknown";
		Log($"State={ctx}, Gen0={gen0}, Gen1={gen1}, Gen2={gen2}, ElapsedSec={elapsed:F1}");
	}

	/// <summary>
	/// Get GC counts since initialization.
	/// </summary>
	public static (int Gen0, int Gen1, int Gen2) GetCurrentCounts()
	{
		if (!_isInitialized)
			Initialize();

		return (
			GC.CollectionCount(0) - _initialGen0Count,
			GC.CollectionCount(1) - _initialGen1Count,
			GC.CollectionCount(2) - _initialGen2Count
		);
	}

	/// <summary>
	/// Log with the [MAUI_GC] prefix that the test infrastructure will parse.
	/// </summary>
	private static void Log(string message)
	{
		// Use Console.WriteLine which goes to logcat on Android
		// The [MAUI_GC] prefix allows the test infrastructure to easily grep these entries
		Console.WriteLine($"[MAUI_GC] {message}");
	}
}

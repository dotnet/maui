#nullable enable
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Controls.Performance.Internals;

/// <summary>
/// Internal performance tracking utility for measuring operations in .NET MAUI.
/// </summary>
[RequiresPerformanceMonitoring("Performance tracker requires performance monitoring")]
internal class Performance
{
	static readonly ConcurrentDictionary<object, Stopwatch> _loadTimers = new();
	
	static readonly object Lock = new object();
	static IPerformanceProfiler? CachedProfiler;

	/// <summary>
	/// Tracks the loading state of an image and performs necessary performance monitoring.
	/// </summary>
	/// <param name="context">The MAUI context containing the service provider</param>
	/// <param name="imageSourcePart">The image source being tracked.</param>
	/// <param name="isLoading">Indicates whether the image is currently loading (<c>true</c>) or has finished (<c>false</c>).</param>
	/// <remarks>
	/// This method monitors image loading behavior, enabling performance tracking and optimization.
	/// It can be used to analyze loading efficiency and improve application responsiveness.
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void TrackImageLoad(IMauiContext? context, object imageSourcePart, bool isLoading)
	{	
		var profiler = GetProfiler(context);
		
		if (profiler?.Image is null)
		{
			return;
		}
		
		if (isLoading)
		{
			// Start timing when loading begins (true)
			var stopwatch = Stopwatch.StartNew();
			_loadTimers.AddOrUpdate(imageSourcePart, stopwatch, (key, existing) =>
			{
				existing?.Stop(); // Stop any existing timer
				return stopwatch;
			});
		}
		else
		{
			// Stop timing when loading ends (false) and record the result
			if (_loadTimers.TryRemove(imageSourcePart, out var stopwatch))
			{
				stopwatch.Stop();
				profiler?.Image?.RecordImageLoad(stopwatch.Elapsed.TotalMilliseconds);
			}
		}
	}
	
	/// <summary>
	/// Tracks the execution time of a measure operation and records the performance data.
	/// </summary>
	/// <typeparam name="T">The return type of the measure function</typeparam>
	/// <param name="context">The MAUI context containing the service provider</param>
	/// <param name="measureFunc">The measure function to execute</param>
	/// <param name="element">The name of the calling element (automatically provided by CallerMemberName)</param>
	/// <returns>The result of the measure function execution</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T TrackMeasure<T>(IMauiContext? context, Func<T> measureFunc, [CallerMemberName] string? element = null)
	{ 
		var profiler = GetProfiler(context);
		
		if (profiler?.Layout is null)
		{
			return measureFunc();
		}

		var stopwatch = Stopwatch.StartNew();
		try
		{
			return measureFunc();
		}
		finally
		{
			stopwatch.Stop();
			profiler?.Layout.RecordMeasurePass(stopwatch.Elapsed.TotalMilliseconds, element);
		}
	}

	/// <summary>
	/// Tracks the execution time of an arrange operation and records the performance data.
	/// </summary>
	/// <typeparam name="T">The return type of the arrange function</typeparam>
	/// <param name="context">The MAUI context containing the service provider</param>
	/// <param name="arrangeFunc">The arrange function to execute</param>
	/// <param name="element">The name of the calling element (automatically provided by CallerMemberName)</param>
	/// <returns>The result of the arrange function execution</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T TrackArrange<T>(IMauiContext? context, Func<T> arrangeFunc, [CallerMemberName] string? element = null)
	{
		var profiler = GetProfiler(context);
		
		if (profiler?.Layout is null)
		{
			return arrangeFunc();
		}

		var stopwatch = Stopwatch.StartNew();
		
		try
		{
			return arrangeFunc();
		}
		finally
		{
			stopwatch.Stop();
			profiler?.Layout.RecordArrangePass(stopwatch.Elapsed.TotalMilliseconds, element);
		}
	}

	/// <summary>
	/// Tracks the execution time of an asynchronous navigation operation and records the performance data.
	/// </summary>
	/// <param name="context">The MAUI context used to retrieve the profiler service instance.</param>
	/// <param name="navigationFunc">The asynchronous navigation function to execute and measure.</param>
	/// <returns>A task representing the asynchronous navigation operation.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static async Task TrackNavigationAsync(
		IMauiContext? context,
		Func<Task> navigationFunc)
	{
		var profiler = GetProfiler(context);
            
		if (profiler?.Navigation is null)
		{
			await navigationFunc();
			return;
		}

		var stopwatch = Stopwatch.StartNew();
		try
		{
			await navigationFunc();
		}
		finally
		{
			stopwatch.Stop();
			
			profiler.Navigation.RecordNavigation(
				stopwatch.Elapsed.TotalMilliseconds);
		}
	}
	
	static IPerformanceProfiler? GetProfiler(IMauiContext? context)
	{
		// Check if the performance profiling feature is enabled.
		if (!PerformanceProfilerFeature.Guard())
		{  
			Application.Current?.FindMauiContext()?.CreateLogger<Performance>()?.LogWarning(
				"MAUI Performance Monitoring is disabled (EnableMauiPerformanceMonitoring)");
			
			return null;
		}
		
		// Return cached profiler if available
		if (CachedProfiler is not null)
		{
			return CachedProfiler;
		}

		lock (Lock)
		{
			// Double-check after acquiring lock
			if (CachedProfiler is not null)
			{
				return CachedProfiler;
			}

			try
			{
				CachedProfiler = context?.Services?.GetService<IPerformanceProfiler>();
				
				return CachedProfiler;
			}
			catch
			{
				return null;
			}
		}
	}
}
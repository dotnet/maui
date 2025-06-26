#nullable enable
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Performance;

/// <summary>
/// Internal performance tracking utility for measuring operations in .NET MAUI.
/// </summary>
internal class PerformanceTracker
{
	static readonly ConcurrentDictionary<object, Stopwatch> _loadTimers = new();
	
	static readonly object Lock = new object();
	static IPerformanceProfiler? CachedProfiler;
	
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
	
	static IPerformanceProfiler? GetProfiler(IMauiContext? context)
	{		
		// Check if the performance profiling feature is enabled.
		if (!PerformanceFeature.Guard())
		{
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
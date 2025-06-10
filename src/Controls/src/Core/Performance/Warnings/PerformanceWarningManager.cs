#nullable disable
using System;

namespace Microsoft.Maui.Controls.Performance;

public class PerformanceWarningManager : IPerformanceWarningManager
{
	WarningOptions _options = new();
	
	public event PerformanceWarningHandler WarningRaised;
	
	/// <summary>
	/// Configures the warnings options for this tracker.
	/// </summary>
	/// <param name="options">Options to control the warnings behavior.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
	public void Configure(WarningOptions options)
	{
		_options = options ?? throw new ArgumentNullException(nameof(options));
	}
	
	/// <summary>
	/// Gets the configured warning options.
	/// </summary>
	public WarningOptions Options => _options;
	
	/// <summary>
	/// Evaluates a metric against a defined threshold and provides a recommendation if applicable.
	/// </summary>
	/// <typeparam name="T">The type of the metric value, which must be comparable.</typeparam>
	/// <param name="category">The category of the metric being checked.</param>
	/// <param name="metric">The specific metric name.</param>
	/// <param name="currentValue">The current value of the metric.</param>
	/// <param name="thresholdValue">The threshold value to compare against.</param>
	/// <param name="recommendation">An optional recommendation message if the threshold is exceeded.</param>
	/// <remarks>
	/// If the current value exceeds the threshold, appropriate action may be taken based on the recommendation.
	/// This method helps monitor and enforce predefined limits for various metrics.
	/// </remarks>
	public void CheckThreshold<T>(string category, string metric, T currentValue, T thresholdValue,
		string recommendation = null) where T : IComparable<T>
	{
		// Ensure performance monitoring is enabled before proceeding
		if (!_options.Enable)
		{
			return;
		}

		// If the current value is within acceptable limits, no action is needed
		if (currentValue.CompareTo(thresholdValue) <= 0)
		{
			return;
		}

		// Determine the severity level of the performance issue
		var level = DetermineWarningLevel(currentValue, thresholdValue);

		// Ignore warnings that don't meet the minimum reporting threshold
		if (level < _options.MinimumLevel)
		{
			return;
		}

		var warning = new PerformanceWarning
		{
			Category = category,
			Metric = metric,
			CurrentValue = currentValue,
			ThresholdValue = thresholdValue,
			Level = level,
			Timestamp = DateTime.UtcNow,
			Message = GenerateWarningMessage(category, metric, currentValue, thresholdValue),
			Recommendation = recommendation ?? GenerateDefaultRecommendation(category, metric),
			Unit = GetUnit(metric)
		};
		
		RaiseWarning(warning);
	}
	
	PerformanceWarningLevel DetermineWarningLevel<T>(T currentValue, T thresholdValue) where T : IComparable<T>
	{
		// Simple heuristic: if current value is more than 2x threshold, it's critical
		if (IsNumericType<T>())
		{
			var current = Convert.ToDouble(currentValue);
			var threshold = Convert.ToDouble(thresholdValue);

			if (current > threshold * 2.0)
			{
				return PerformanceWarningLevel.Critical;
			}

			if (current > threshold * 1.5)
			{
				return PerformanceWarningLevel.Warning;
			}
		}
		
		return PerformanceWarningLevel.Info;
	}

	string GenerateWarningMessage<T>(string category, string metric, T currentValue, T thresholdValue)
	{
		var unit = GetUnit(metric);
		var currentStr = FormatValue(currentValue, unit);
		var thresholdStr = FormatValue(thresholdValue, unit);

		return $"{category} {metric}: {currentStr} > {thresholdStr}";
	}

	string GenerateDefaultRecommendation(string category, string metric)
	{
		return (category, metric) switch
		{
			("Layout", var m) when m.IndexOf("Time", StringComparison.OrdinalIgnoreCase) >= 0 =>
				"Consider reducing layout complexity or using more efficient layout containers",
			_ => "Review the performance characteristics of this operation"
		};
	}

	string GetUnit(string metric)
	{
		if (metric.IndexOf("Duration", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return "ms";
		}

		if (metric.IndexOf("Count", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return "count";
		}

		return string.Empty;
	}

	string FormatValue<T>(T value, string unit)
	{
		if (unit == "ms" && IsNumericType<T>())
		{
			var ms = Convert.ToDouble(value);
			return ms >= 1000 ? $"{ms / 1000:F2}s" : $"{ms:F2}ms";
		}
		
		return value?.ToString() ?? "null";
	}
	
	void RaiseWarning(PerformanceWarning warning)
	{
		try
		{
			WarningRaised?.Invoke(this, new PerformanceWarningEventArgs(warning));
		}
		catch (Exception ex)
		{
			// Log exception but don't let it crash the performance monitoring
			System.Diagnostics.Debug.WriteLine($"Error raising performance warning: {ex.Message}");
		}
	}
	
	static bool IsNumericType<T>()
	{
		return typeof(T) == typeof(int) || typeof(T) == typeof(long) ||
		       typeof(T) == typeof(float) || typeof(T) == typeof(double) ||
		       typeof(T) == typeof(decimal);
	}
}
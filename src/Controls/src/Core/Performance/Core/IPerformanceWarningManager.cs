using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Performance
{
	/// <summary>
	/// Manages performance warning thresholds and raises events when metrics exceed configured limits.
	/// </summary>
	public interface IPerformanceWarningManager
	{
		/// <summary>
		/// Gets the configured layout tracking options.
		/// </summary>
		WarningOptions Options { get; }
		
		/// <summary>
		/// Occurs when a performance metric crosses its defined threshold.
		/// </summary>
		/// <remarks>
		/// Subscribing to this event allows external components to react to performance warnings.
		/// Implementations may log the warning, adjust behavior, or notify users.
		/// </remarks>
		event PerformanceWarningHandler WarningRaised;
		
		/// <summary>
		/// Configures the warnings behavior using the provided options.
		/// </summary>
		/// <param name="options">The warnings options to apply.</param>
		void Configure(WarningOptions options);
		
		/// <summary>
		/// Checks a given metric value against its threshold and raises <see cref="WarningRaised"/>
		/// if the threshold is exceeded.
		/// </summary>
		/// <typeparam name="T">A comparable type for the metric values (e.g., <see cref="int"/>, <see cref="double"/>).</typeparam>
		/// <param name="category">The category of the metric (e.g., "Layout", "Image").</param>
		/// <param name="metric">The specific metric name within the category (e.g., "MeasureTime").</param>
		/// <param name="currentValue">The current observed value of the metric.</param>
		/// <param name="thresholdValue">The configured threshold value for this metric.</param>
		/// <param name="recommendation">
		/// An optional suggestion or remediation note to include with the warning.
		/// </param>
		void CheckThreshold<T>(
			string category,
			string metric,
			T currentValue,
			T thresholdValue,
			string? recommendation = null)
			where T : IComparable<T>;
	}

	/// <summary>
	/// Signature for callbacks when a performance warning is raised.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">Contains the raised warning.</param>
	public delegate void PerformanceWarningHandler(object sender, PerformanceWarningEventArgs e);

	/// <summary>
	/// Carries information about a single performance warning.
	/// </summary>
	public class PerformanceWarningEventArgs : EventArgs
	{
		/// <summary>
		/// The warning that was raised.
		/// </summary>
		public PerformanceWarning Warning { get; }

		public PerformanceWarningEventArgs(PerformanceWarning warning)
		{
			Warning = warning ?? throw new ArgumentNullException(nameof(warning));
		}
	}
}
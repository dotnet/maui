using System;

namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Defines an interface for tracking the performance of image loading operations.
/// </summary>
public interface IImagePerformanceTracker
{
	/// <summary>
	/// Gets the current image tracking options.
	/// </summary>
	/// <remarks>
	/// This property provides access to the configured settings that control 
	/// how image load times are monitored.
	/// </remarks>
	ImageTrackingOptions Options { get; }

	/// <summary>
	/// Configures image tracking behavior using the provided options.
	/// </summary>
	/// <param name="options">The configuration options for tracking image load performance.</param>
	/// <remarks>
	/// Allows customization of image performance tracking, enabling fine-tuned monitoring 
	/// based on application needs.
	/// </remarks>
	void Configure(ImageTrackingOptions options);

	/// <summary>
	/// Retrieves statistical data related to image load performance.
	/// </summary>
	/// <returns>An <see cref="ImageStats"/> object containing performance metrics.</returns>
	/// <remarks>
	/// This method provides insights into how images are loaded and rendered, 
	/// helping optimize performance.
	/// </remarks>
	ImageStats GetStats();

	/// <summary>
	/// Subscribes to image load updates for real-time monitoring.
	/// </summary>
	/// <param name="callback">An action to execute when an image update occurs.</param>
	/// <remarks>
	/// This allows external components to react to changes in image load performance, 
	/// such as logging or UI updates.
	/// </remarks>
	void SubscribeToImageUpdates(Action<ImageUpdate> callback);

	/// <summary>
	/// Records the duration of an image load event, triggering updates and warnings if needed.
	/// </summary>
	/// <param name="loadDuration">The measured load duration in milliseconds.</param>
	void RecordImageLoad(double loadDuration);
}

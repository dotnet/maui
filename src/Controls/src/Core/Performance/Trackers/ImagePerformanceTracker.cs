using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace Microsoft.Maui.Controls.Performance;

public class ImagePerformanceTracker : IImagePerformanceTracker
{
	readonly Histogram<double> _loadDurationHistogram;

	readonly IPerformanceWarningManager _warningManager;
	
	double _loadDuration;

	// List of subscribers for image‐update callbacks
	readonly List<Action<ImageUpdate>> _subscribers = new List<Action<ImageUpdate>>();
	readonly object _subscriberLock = new object();

	ImageTrackingOptions _options = new();

	public ImagePerformanceTracker(Meter meter, IPerformanceWarningManager? warningManager = null)
	{
		_loadDurationHistogram = meter.CreateHistogram<double>(
			name: "maui.image.load.duration",
			unit: "ms",
			description: "Image load duration in milliseconds");
		
		_warningManager = warningManager ?? new PerformanceWarningManager();
	}

	/// <summary>
	/// Configures image tracking behavior using the provided options.
	/// </summary>
	/// <param name="options">The configuration options for tracking image load performance.</param>
	public void Configure(ImageTrackingOptions options)
	{
		_options = options ?? throw new ArgumentNullException(nameof(options));
	}

	/// <summary>
	/// Gets the current image tracking options.
	/// </summary>
	public ImageTrackingOptions Options => _options;

	/// <summary>
	/// Records the duration of an image load event, triggering updates and warnings if needed.
	/// </summary>
	/// <param name="loadDuration">The measured load duration in milliseconds.</param>
	public void RecordImageLoad(double loadDuration)
	{
		if (!_options.EnableLoadTimeTracking)
		{
			return;
		}

		// Update fields
		_loadDuration = loadDuration;

		_loadDurationHistogram.Record(loadDuration);
		
		// Notify subscribers
		var update = new ImageUpdate
		{
			TotalTime = _loadDuration,
			TimestampUtc = DateTime.UtcNow
		};
		
		PublishImageUpdate(update);
		
		// Check layout image and generate warnings
		CheckImageThresholds(loadDuration);
	}

	/// <summary>
	/// Retrieves statistical data related to image load performance.
	/// </summary>
	/// <returns>An <see cref="ImageStats"/> object containing performance metrics.</returns>
	public ImageStats GetStats()
	{
		return new ImageStats { LoadDuration = _loadDuration, };
	}
	
	/// <summary>
	/// Subscribes to image load updates for real-time monitoring.
	/// </summary>
	/// <param name="callback">An action to execute when an image update occurs.</param>
	public void SubscribeToImageUpdates(Action<ImageUpdate> callback)
	{
		if (callback == null)
		{
			throw new ArgumentNullException(nameof(callback));
		}

		lock (_subscriberLock)
		{
			_subscribers.Add(callback);
		}
	}
	
	/// <summary>
	/// Notifies subscribers with the latest image performance update.
	/// </summary>
	/// <param name="update">The image update data containing load duration and timestamp.</param>
	void PublishImageUpdate(ImageUpdate update)
	{
		Action<ImageUpdate>[] subscribersSnapshot;
		lock (_subscriberLock)
		{
			subscribersSnapshot = _subscribers.ToArray();
		}

		foreach (var subscriber in subscribersSnapshot)
		{
			try
			{
				subscriber.Invoke(update);
			}
			catch
			{
				// Swallow exceptions from subscribers to avoid breaking the tracker.
			}
		}
	}
	
	/// <summary>
	/// Checks if the image load duration exceeds defined thresholds and raises warnings if necessary.
	/// </summary>
	/// <param name="duration">The duration of the image load event.</param>
	void CheckImageThresholds(double duration)
	{
		const string category = "Image";

		var warningOptions = _warningManager.Options;
		ImageThresholds imageThresholds = warningOptions.Thresholds.Image;
			
		_warningManager.CheckThreshold(
			category,
			"ImageLoadDuration",
			duration,
			imageThresholds.ImageMaxLoadTime.TotalMilliseconds,
			$"Image load time took {duration}ms. Consider optimizing image size, format, or caching strategies to improve performance.");
	}
}
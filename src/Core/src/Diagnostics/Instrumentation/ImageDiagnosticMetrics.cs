using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Microsoft.Maui.Diagnostics;

/// <summary>
/// Defines comprehensive diagnostic metrics for image loading operations, including load duration, 
/// file sizes, pixel dimensions, memory usage, and failure tracking.
/// </summary>
internal class ImageDiagnosticMetrics : IDiagnosticMetrics
{
    /// <summary>
    /// Gets the histogram metric for the total duration of image loading operations in milliseconds.
    /// </summary>
    public Histogram<long>? ImageLoadDuration { get; private set; }
    
    /// <summary>
    /// Gets the histogram metric for the file size of loaded images in bytes.
    /// </summary>
    public Histogram<long>? ImageFileSize { get; private set; }
    
    /// <summary>
    /// Gets the histogram metric for image width in pixels.
    /// </summary>
    public Histogram<int>? ImageWidth { get; private set; }
    
    /// <summary>
    /// Gets the histogram metric for image height in pixels.
    /// </summary>
    public Histogram<int>? ImageHeight { get; private set; }
    
    /// <summary>
    /// Gets the counter metric for successful image loads.
    /// </summary>
    public Counter<int>? ImageLoadSuccess { get; private set; }
    
    /// <summary>
    /// Gets the counter metric for image load failures.
    /// </summary>
    public Counter<int>? ImageLoadFailures { get; private set; }

    /// <inheritdoc/>
    public void Create(Meter meter)
    {
        ImageLoadDuration = meter.CreateHistogram<long>(
            "maui.image.load_duration", 
            "ms", 
            "Total duration of image loading operations from start to completion");
            
        ImageFileSize = meter.CreateHistogram<long>(
            "maui.image.file_size", 
            "bytes", 
            "Original file size of loaded images");
            
        ImageWidth = meter.CreateHistogram<int>(
            "maui.image.width", 
            "pixels", 
            "Width of loaded images in pixels");
            
        ImageHeight = meter.CreateHistogram<int>(
            "maui.image.height", 
            "pixels", 
            "Height of loaded images in pixels");
            
        ImageLoadSuccess = meter.CreateCounter<int>(
            "maui.image.load_success", 
            "{loads}", 
            "Number of successful image load operations");
            
        ImageLoadFailures = meter.CreateCounter<int>(
            "maui.image.load_failures", 
            "{failures}", 
            "Number of image load failures");
    }

    /// <summary>
    /// Records a successful image loading operation with comprehensive metrics.
    /// </summary>
    /// <param name="tagList">The tags associated with the image loading operation.</param>
    /// <param name="duration">The total duration of the image loading operation in milliseconds.</param>
    /// <param name="fileSizeBytes">The original file size of the image in bytes (0 if unknown).</param>
    /// <param name="widthPixels">The width of the image in pixels (0 if unknown).</param>
    /// <param name="heightPixels">The height of the image in pixels (0 if unknown).</param>
    public void RecordImageLoadSuccess(
        in TagList tagList, 
        long duration, 
        long fileSizeBytes = 0, 
        int widthPixels = 0,
        int heightPixels = 0)
    {
        // Always record duration and success count
        ImageLoadDuration?.Record(duration, tagList);
        ImageLoadSuccess?.Add(1, tagList);

        // Record file size if available
        if (fileSizeBytes > 0)
        {
            ImageFileSize?.Record(fileSizeBytes, tagList);
        }
        
        // Record dimensions if available
        if (widthPixels > 0)
        {
            ImageWidth?.Record(widthPixels, tagList);
        }
        
        if (heightPixels > 0)
        {
            ImageHeight?.Record(heightPixels, tagList);
        }
    }
    
    /// <summary>
    /// Records an image load failure with error details.
    /// </summary>
    /// <param name="tagList">The tags associated with the failed operation.</param>
    /// <param name="durationMs">The duration before failure occurred in milliseconds.</param>
    /// <param name="errorType">The type of error that occurred.</param>
    /// <param name="errorMessage">The error message (optional, for debugging).</param>
    public void RecordImageLoadFailure(
        in TagList tagList, 
        long durationMs = 0,
        string? errorType = null,
        string? errorMessage = null)
    {
        // Create extended tag list with error information
        var extendedTagList = new TagList();
        
        foreach (var tag in tagList)
        {
            extendedTagList.Add(tag);
        }
        
        // Add error information to tags
        if (!string.IsNullOrEmpty(errorType))
        {
            extendedTagList.Add("error.type", errorType);
        }
        
        if (!string.IsNullOrEmpty(errorMessage))
        {
            extendedTagList.Add("error.message", errorMessage);
        }

        ImageLoadFailures?.Add(1, extendedTagList);
        
        // Record duration even for failures to understand timing patterns
        if (durationMs > 0)
        {
            ImageLoadDuration?.Record(durationMs, extendedTagList);
        }
    }
}
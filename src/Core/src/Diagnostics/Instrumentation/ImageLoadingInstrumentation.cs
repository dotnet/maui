using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

#if ANDROID
using Android.Widget;
using Android.Graphics.Drawables;
using AndroidPath = Android.Graphics.Path;
#elif IOS || MACCATALYST
using UIKit;
using CoreGraphics;
using Foundation;
#elif WINDOWS
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
#endif

namespace Microsoft.Maui.Diagnostics;

/// <summary>
/// Enhanced instrumentation for measuring comprehensive image loading operations in a view.
/// </summary>
readonly struct ImageLoadingInstrumentation : IDiagnosticInstrumentation
{
	readonly IView _view;
	readonly Activity? _activity;
	readonly long _startTime;
	readonly IImageSource? _imageSource;
	readonly ImageLoadingState _state;

	/// <summary>
	/// Initializes a new instance of the <see cref="ImageLoadingInstrumentation"/> struct.
	/// </summary>
	/// <param name="view">The view being instrumented.</param>
	public ImageLoadingInstrumentation(IView view)
	{
		_view = view;
		_imageSource = (view as IImage)?.Source;
		_activity = view.StartDiagnosticActivity($"Image.Load");
		_startTime = Stopwatch.GetTimestamp();
		_state = new ImageLoadingState();
	}

	/// <summary>
	/// Records that the image was loaded from cache.
	/// </summary>
	public void RecordCacheHit()
	{
		_state.WasFromCache = true;
	}

	/// <summary>
	/// Records an exception that occurred during loading.
	/// </summary>
	/// <param name="exception">The exception that occurred.</param>
	public void RecordException(Exception exception)
	{
		_state.LoadException = exception;
	}

	/// <summary>
	/// Records file size information.
	/// </summary>
	/// <param name="sizeBytes">The file size in bytes.</param>
	public void RecordFileSize(long sizeBytes)
	{
		_state.FileSizeBytes = sizeBytes;
	}

	/// <summary>
	/// Records image dimensions.
	/// </summary>
	/// <param name="width">Width in pixels.</param>
	/// <param name="height">Height in pixels.</param>
	public void RecordDimensions(int width, int height)
	{
		_state.WidthPixels = width;
		_state.HeightPixels = height;
	}

	/// <summary>
	/// Disposes the instrumentation and stops the diagnostic activity.
	/// </summary>
	public void Dispose() =>
		_view.StopDiagnostics(_activity, this);

	/// <summary>
	/// Records the stopping of the instrumentation and publishes comprehensive image loading metrics.
	/// </summary>
	/// <param name="diagnostics">The <see cref="IDiagnosticsManager"/> instance.</param>
	/// <param name="tagList">The tags associated with the instrumentation.</param>
	public void Stopped(IDiagnosticsManager diagnostics, in TagList tagList)
	{
		long durationMs = 0;
		
#if !NETSTANDARD
		durationMs = (long)Stopwatch.GetElapsedTime(_startTime).TotalMilliseconds;
#endif

		var metrics = diagnostics.GetMetrics<ImageDiagnosticMetrics>();
		if (metrics is null)
			return;

		// Create extended tag list with comprehensive image loading information
		var extendedTagList = CreateExtendedTagList(tagList);

		// If we haven't captured dimensions yet, try to get them from the platform view
		if (_state.WidthPixels == 0 || _state.HeightPixels == 0)
		{
			var platformDimensions = GetPlatformImageDimensions();
			if (platformDimensions.HasValue)
			{
				_state.WidthPixels = (int)platformDimensions.Value.Width;
				_state.HeightPixels = (int)platformDimensions.Value.Height;
			}
		}

		// If we haven't captured file size yet, try to estimate it
		if (_state.FileSizeBytes == 0)
		{
			_state.FileSizeBytes = EstimateFileSize();
		}

		// Record success or failure metrics
		if (_state.LoadException == null)
		{
			// Success case
			metrics.RecordImageLoadSuccess(
				extendedTagList,
				durationMs,
				_state.FileSizeBytes,
				EstimateMemorySize(_state.WidthPixels, _state.HeightPixels),
				_state.WidthPixels,
				_state.HeightPixels,
				_state.WasFromCache);
		}
		else
		{
			// Failure case
			var errorType = GetErrorType(_state.LoadException);
			metrics.RecordImageLoadFailure(
				extendedTagList,
				durationMs,
				errorType,
				_state.LoadException.Message);
		}
	}

	TagList CreateExtendedTagList(in TagList originalTagList)
	{
		var extendedTagList = new TagList();
		
		// Copy original tags
		foreach (var tag in originalTagList)
		{
			extendedTagList.Add(tag);
		}
		
		// Add image-specific tags
		if (_view is IImage image)
		{
			extendedTagList.Add("control.type", "Image");
			
			// Add image format
			var format = GetImageFormat(image.Source);
			if (!string.IsNullOrEmpty(format))
			{
				extendedTagList.Add("image.format", format);
			}
			
			// Add size category
			var sizeCategory = GetImageSizeCategory(_state.WidthPixels, _state.HeightPixels);
			if (!string.IsNullOrEmpty(sizeCategory))
			{
				extendedTagList.Add("image.size_category", sizeCategory);
			}

			// Add source type
			var sourceType = GetImageSourceType(image.Source);
			if (!string.IsNullOrEmpty(sourceType))
			{
				extendedTagList.Add("image.source_type", sourceType);
			}

			// Add aspect ratio category
			var aspectCategory = GetAspectRatioCategory(_state.WidthPixels, _state.HeightPixels);
			if (!string.IsNullOrEmpty(aspectCategory))
			{
				extendedTagList.Add("image.aspect_category", aspectCategory);
			}
		}

		return extendedTagList;
	}

	static string GetErrorType(Exception exception)
	{
		return exception switch
		{
			FileNotFoundException => "FileNotFound",
			UnauthorizedAccessException => "UnauthorizedAccess",
			HttpRequestException => "NetworkError",
			TaskCanceledException => "Timeout",
			OutOfMemoryException => "OutOfMemory",
			ArgumentException => "InvalidArgument",
			_ => exception.GetType().Name
		};
	}

	static string GetImageFormat(IImageSource? source)
	{
		return source switch
		{
			IFileImageSource fileSource => GetFormatFromPath(fileSource.File),
			IUriImageSource uriSource => GetFormatFromUri(uriSource.Uri),
			IStreamImageSource => "Stream",
			IFontImageSource => "FontIcon",
			_ => "Unknown"
		};
	}

	static string GetImageSourceType(IImageSource? source)
	{
		return source switch
		{
			IFileImageSource => "File",
			IUriImageSource uriSource => uriSource.Uri?.Scheme?.ToUpperInvariant() switch
			{
				"HTTP" or "HTTPS" => "Network",
				"FILE" => "LocalFile",
				_ => "Uri"
			},
			IStreamImageSource => "Stream",
			IFontImageSource => "FontIcon",
			_ => "Unknown"
		};
	}

	static string GetFormatFromPath(string? path)
	{
		if (string.IsNullOrEmpty(path))
			return "Unknown";

		var extension = System.IO.Path.GetExtension(path).ToLowerInvariant();
		return extension switch
		{
			".jpg" or ".jpeg" => "JPEG",
			".png" => "PNG",
			".webp" => "WebP",
			".gif" => "GIF",
			".heic" or ".heif" => "HEIC",
			".bmp" => "BMP",
			".svg" => "SVG",
			".tiff" or ".tif" => "TIFF",
			".ico" => "ICO",
			_ => "Unknown"
		};
	}

	static string GetFormatFromUri(Uri? uri)
	{
		if (uri is null)
			return "Unknown";

		return GetFormatFromPath(uri.AbsolutePath);
	}
	
	static string GetImageSizeCategory(int width, int height)
	{
		if (width <= 0 || height <= 0)
			return "Unknown";

		var area = (long)width * height;
		return area switch
		{
			<= 10000 => "Thumbnail",     // ≤100x100
			<= 250000 => "Small",        // ≤500x500  
			<= 2000000 => "Medium",      // ≤1920x1080 (Full HD)
			<= 8000000 => "Large",       // ≤4K
			_ => "ExtraLarge"            // >4K
		};
	}

	static string GetAspectRatioCategory(int width, int height)
	{
		if (width <= 0 || height <= 0)
			return "Unknown";

		var ratio = (double)width / height;
		return ratio switch
		{
			< 0.7 => "Portrait",        // Tall images
			> 1.4 => "Landscape",       // Wide images  
			_ => "Square"               // Roughly square
		};
	}

	(double Width, double Height)? GetPlatformImageDimensions()
	{
		if (_view is IView view && view.Handler is Handlers.IImageHandler handler)
		{
			return GetPlatformImageDimensions(handler);
		}
		return null;
	}

	long EstimateFileSize()
	{
		// Try to get file size based on source type
		if (_imageSource is IFileImageSource fileSource)
		{
			return GetLocalFileSize(fileSource.File);
		}

		// For other sources, we can't easily determine file size without loading
		return 0;
	}

	static long GetLocalFileSize(string? filePath)
	{
		if (string.IsNullOrEmpty(filePath))
			return 0;

		try
		{
			var fileInfo = new FileInfo(filePath);
			return fileInfo.Exists ? fileInfo.Length : 0;
		}
		catch
		{
			return 0;
		}
	}

	static long EstimateMemorySize(int width, int height)
	{
		if (width <= 0 || height <= 0)
			return 0;

		// Estimate memory usage: width * height * 4 bytes per pixel (RGBA)
		return (long)width * height * 4;
	}

#if ANDROID
	static (double Width, double Height)? GetPlatformImageDimensions(Handlers.IImageHandler handler)
	{
		if (handler.PlatformView is ImageView imageView && 
			imageView.Drawable is not null)
		{
			var drawable = imageView.Drawable;
			return (drawable.IntrinsicWidth, drawable.IntrinsicHeight);
		}
		return null;
	}
#elif IOS || MACCATALYST
	static (double Width, double Height)? GetPlatformImageDimensions(Handlers.IImageHandler handler)
	{
		if (handler.PlatformView is UIKit.UIImageView imageView && 
			imageView.Image is not null)
		{
			var image = imageView.Image;
			return (image.Size.Width * image.CurrentScale, image.Size.Height * image.CurrentScale);
		}
		return null;
	}
#elif WINDOWS
	static (double Width, double Height)? GetPlatformImageDimensions(Handlers.IImageHandler handler)
	{
		if (handler.PlatformView is Microsoft.UI.Xaml.Controls.Image image && 
			image.Source is Microsoft.UI.Xaml.Media.Imaging.BitmapSource bitmapSource)
		{
			return (bitmapSource.PixelWidth, bitmapSource.PixelHeight);
		}
		return null;
	}
#else
	static (double Width, double Height)? GetPlatformImageDimensions(Handlers.IImageHandler handler)
	{
		// Fallback for other platforms
		return null;
	}
#endif
}

/// <summary>
/// Mutable state container for image loading instrumentation data.
/// </summary>
internal class ImageLoadingState
{
	public bool WasFromCache { get; set; }
	public Exception? LoadException { get; set; }
	public long FileSizeBytes { get; set; }
	public int WidthPixels { get; set; }
	public int HeightPixels { get; set; }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

#if ANDROID
using Android.Content.Res;
using Android.Widget;
using Android.Graphics.Drawables;
using Android.Graphics;
using Java.IO;
using AApplication = Android.App.Application;
#elif IOS || MACCATALYST
using UIKit;
using CoreGraphics;
using Foundation;
#elif WINDOWS
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
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
		{
			return;
		}

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
			_state.FileSizeBytes = GetFileSize();
		}

		// Record success or failure metrics
		if (_state.LoadException is null)
		{
			// Success
			metrics.RecordImageLoadSuccess(
				extendedTagList,
				durationMs,
				_state.FileSizeBytes,
				_state.WidthPixels,
				_state.HeightPixels);
		}
		else
		{
			// Failure
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
			System.IO.FileNotFoundException => "FileNotFound",
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
		{
			return "Unknown";
		}

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
		{
			return "Unknown";
		}

		return GetFormatFromPath(uri.AbsolutePath);
	}
	
	static string GetImageSizeCategory(int width, int height)
	{
		if (width <= 0 || height <= 0)
		{
			return "Unknown";
		}

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
		{
			return "Unknown";
		}

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

	long GetFileSize()
	{
		try
		{
			return _imageSource switch
			{
				IFileImageSource fileSource => GetLocalFileSize(fileSource.File),
				IUriImageSource uriSource => GetUriImageSize(uriSource.Uri),
				IStreamImageSource streamSource => GetStreamImageSize(streamSource),
				_ => GetPlatformImageSize()
			};
		}
		catch (Exception)
		{
			// If any method fails, try to get size from platform view as fallback
			return GetPlatformImageSize();
		}
	}

	static long GetLocalFileSize(string? filePath)
	{
		if (string.IsNullOrEmpty(filePath))
		{
			return 0;
		}

		try
		{
			// Return immediately if filePath is not valid
			if (string.IsNullOrEmpty(filePath))
			{
				return 0;
			}

			var normalizedPath = filePath;

			// Remove URI scheme for local files
			if (filePath!.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
			{
				normalizedPath = filePath.Substring(7);
			}

			// Return if normalized path is not valid
			if (string.IsNullOrEmpty(normalizedPath))
			{
				return 0;
			}

			// Check if it's an embedded resource
			if (IsEmbeddedResource(normalizedPath!))
			{
				return GetEmbeddedResourceSize(normalizedPath);
			}
	
			// Regular file system file
			var fileInfo = new FileInfo(normalizedPath);
			return fileInfo.Exists ? fileInfo.Length : 0;
		}
		catch
		{
			return 0;
		}
	}

	static long GetUriImageSize(Uri? uri)
	{
		if (uri is null)
		{
			return 0;
		}

		try
		{
			// Handle different URI schemes
			return uri.Scheme.ToLowerInvariant() switch
			{
				"file" => GetLocalFileSize(uri.LocalPath),
				"http" or "https" => GetRemoteImageSize(uri),
				"ms-appx" => GetAppResourceSize(uri),
				"ms-appdata" => GetAppDataSize(uri),
				_ => 0
			};
		}
		catch
		{
			return 0;
		}
	}

	static long GetStreamImageSize(IStreamImageSource streamSource)
	{
		try
		{
			// If we can get the stream, measure its length
			var stream = streamSource.GetStreamAsync(System.Threading.CancellationToken.None).Result;
			if (stream?.CanSeek == true)
			{
				var length = stream.Length;
				stream.Position = 0; // Reset position to not interfere with image loading
				return length;
			}
		}
		catch
		{
			// Stream might not be seekable or accessible
		}
		return 0;
	}
	
	long GetPlatformImageSize()
	{
		if (_view is IView view && view.Handler is Handlers.IImageHandler handler)
		{
			return GetPlatformImageSize(handler);
		}
		return 0;
	}

	static bool IsEmbeddedResource(string? path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}

		// Check common patterns for embedded resources
#if NETSTANDARD
		return false;
#else
		return path is not null && path.Contains('.', StringComparison.OrdinalIgnoreCase) && !System.IO.Path.IsPathRooted(path) &&
		       !path.StartsWith("/", StringComparison.OrdinalIgnoreCase);
#endif
	}

	static long GetEmbeddedResourceSize(string? resourcePath)
	{
		if (string.IsNullOrEmpty(resourcePath))
		{
			return 0;
		}
#if ANDROID
		return GetAndroidAssetSize(resourcePath);
#elif IOS || MACCATALYST
		return GetIOSBundleResourceSize(resourcePath);
#elif WINDOWS
		return GetWindowsAppResourceSize(resourcePath);
#else
		return 0;
#endif
	}

	static long GetRemoteImageSize(Uri uri)
	{
		try
		{
			// For remote images, we could make a HEAD request to get Content-Length
			// However, this would be async and might impact performance
			// For now, return 0 and let the platform size detection handle it
			return 0;
		}
		catch
		{
			return 0;
		}
	}

	static long GetAppResourceSize(Uri uri)
	{
#if WINDOWS
		return GetWindowsAppResourceSize(uri.LocalPath);
#else
		return 0;
#endif
	}

	static long GetAppDataSize(Uri uri)
	{
#if WINDOWS
		return GetWindowsAppDataSize(uri.LocalPath);
#else
		return 0;
#endif
	}

#if ANDROID
	static long GetAndroidAssetSize(string assetPath)
	{
		if (string.IsNullOrEmpty(assetPath))
		{
			return 0;
		}

		try
		{
			var context = ApplicationModel.Platform.CurrentActivity ?? AApplication.Context;
			using var inputStream = context.Assets?.Open(assetPath);
			if (inputStream is null)
			{
				return 0;
			}

			long total = 0;
			byte[] buffer = new byte[8192];
			int read;
			while ((read = inputStream.Read(buffer, 0, buffer.Length)) > 0)
			{
				total += read;
			}

			return total;
		}
		catch
		{
			return 0;
		}
	}

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

	static long GetPlatformImageSize(Handlers.IImageHandler handler)
	{
		try
		{
			if (handler.PlatformView is ImageView imageView && imageView.Drawable is BitmapDrawable bitmapDrawable)
			{
				var bitmap = bitmapDrawable.Bitmap;
				if (bitmap is not null && !bitmap.IsRecycled)
				{
					return bitmap.ByteCount;
				}
			}
		}
		catch
		{
			// Ignore errors
		}
		return 0;
	}

#elif IOS || MACCATALYST
	static long GetIOSBundleResourceSize(string resourcePath)
	{
		try
		{
			var bundle = NSBundle.MainBundle;
			var resourceUrl = bundle.GetUrlForResource(Path.GetFileNameWithoutExtension(resourcePath),
				Path.GetExtension(resourcePath)?.TrimStart('.') ?? string.Empty);
			
			var filePath = resourceUrl.Path;
			
			if (filePath is not null)
			{
				var fileInfo = new FileInfo(filePath);
				return fileInfo.Exists ? fileInfo.Length : 0;
			}
		}
		catch
		{
			// Ignore errors
		}

		return 0;
	}

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

	static long GetPlatformImageSize(Handlers.IImageHandler handler)
	{
		try
		{
			if (handler.PlatformView is UIImageView imageView && imageView.Image is not null)
			{
				var image = imageView.Image;
				// Estimate size: width * height * 4 bytes per pixel (RGBA)
				var pixelCount = (long)(image.Size.Width * image.CurrentScale * image.Size.Height * image.CurrentScale);
				return pixelCount * 4;
			}
		}
		catch
		{
			// Ignore errors
		}
		return 0;
	}

#elif WINDOWS
	static long GetWindowsAppResourceSize(string resourcePath)
	{
		try
		{
			var uri = new Uri($"ms-appx:///{resourcePath.TrimStart('/')}");
			var file = StorageFile.GetFileFromApplicationUriAsync(uri).AsTask().Result;
			var properties = file.GetBasicPropertiesAsync().AsTask().Result;
			return (long)properties.Size;
		}
		catch
		{
			return 0;
		}
	}

	static long GetWindowsAppDataSize(string resourcePath)
	{
		try
		{
			var uri = new Uri($"ms-appdata:///{resourcePath.TrimStart('/')}");
			var file = StorageFile.GetFileFromApplicationUriAsync(uri).AsTask().Result;
			var properties = file.GetBasicPropertiesAsync().AsTask().Result;
			return (long)properties.Size;
		}
		catch
		{
			return 0;
		}
	}

	static (double Width, double Height)? GetPlatformImageDimensions(Handlers.IImageHandler handler)
	{
		if (handler.PlatformView is Microsoft.UI.Xaml.Controls.Image image && 
			image.Source is Microsoft.UI.Xaml.Media.Imaging.BitmapSource bitmapSource)
		{
			return (bitmapSource.PixelWidth, bitmapSource.PixelHeight);
		}
		return null;
	}

	static long GetPlatformImageSize(Handlers.IImageHandler handler)
	{
		try
		{
			if (handler.PlatformView is Microsoft.UI.Xaml.Controls.Image image)
			{
				return image.Source switch
				{
					BitmapImage bitmapImage => GetBitmapImageSize(bitmapImage),
					WriteableBitmap writeableBitmap => GetWriteableBitmapSize(writeableBitmap),
					_ => 0
				};
			}
		}
		catch
		{
			// Ignore errors
		}
		return 0;
	}

	static long GetBitmapImageSize(BitmapImage bitmapImage)
	{
		try
		{
			// Estimate size based on pixel dimensions
			var pixelCount = (long)bitmapImage.PixelWidth * bitmapImage.PixelHeight;
			return pixelCount * 4; // 4 bytes per pixel (RGBA)
		}
		catch
		{
			return 0;
		}
	}

	static long GetWriteableBitmapSize(WriteableBitmap writeableBitmap)
	{
		try
		{
			// WriteableBitmap has a PixelBuffer property
			return (long)writeableBitmap.PixelBuffer.Length;
		}
		catch
		{
			return 0;
		}
	}

#else
	static (double Width, double Height)? GetPlatformImageDimensions(Handlers.IImageHandler handler)
	{
		// Fallback for other platforms
		return null;
	}

	static long GetPlatformImageSize(Handlers.IImageHandler handler)
	{
		// Fallback for other platforms
		return 0;
	}
#endif
}

/// <summary>
/// Mutable state container for image loading instrumentation data.
/// </summary>
internal class ImageLoadingState
{
	public Exception? LoadException { get; set; }
	public long FileSizeBytes { get; set; }
	public int WidthPixels { get; set; }
	public int HeightPixels { get; set; }
}
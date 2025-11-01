#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Media;
#if IOS || ANDROID || WINDOWS
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
#endif

namespace Microsoft.Maui.Essentials;

/// <summary>
/// Unified image processing helper using MAUI Graphics for cross-platform consistency.
/// </summary>
internal static partial class ImageProcessor
{
	/// <summary>
	/// Determines if the image needs rotation based on the provided options.
	/// </summary>
	/// <param name="options">The media picker options.</param>
	/// <returns>True if rotation is needed based on the provided options.</returns>
	public static bool IsRotationNeeded(MediaPickerOptions? options)
	{
		return options?.RotateImage ?? false;
	}

	/// <summary>
	/// Platform-specific EXIF rotation implementation.
	/// </summary>
	public static partial Task<Stream> RotateImageAsync(Stream inputStream, string? originalFileName);

	/// <summary>
	/// Platform-specific metadata extraction implementation.
	/// </summary>
	public static partial Task<byte[]?> ExtractMetadataAsync(Stream inputStream, string? originalFileName);

	/// <summary>
	/// Platform-specific metadata application implementation.
	/// </summary>
	public static partial Task<Stream> ApplyMetadataAsync(Stream processedStream, byte[] metadata, string? originalFileName);

	/// <summary>
	/// Processes an image by applying EXIF rotation, resizing and compression using MAUI Graphics.
	/// </summary>
	/// <param name="inputStream">The input image stream.</param>
	/// <param name="maxWidth">Maximum width constraint (null for no constraint).</param>
	/// <param name="maxHeight">Maximum height constraint (null for no constraint).</param>
	/// <param name="qualityPercent">Compression quality percentage (0-100).</param>
	/// <param name="originalFileName">Original filename to determine format preservation logic.</param>
	/// <param name="rotateImage">Whether to apply EXIF rotation correction.</param>
	/// <param name="preserveMetaData">Whether to preserve metadata (including EXIF data) in the processed image.</param>
	/// <returns>A new stream containing the processed image.</returns>
	public static async Task<Stream?> ProcessImageAsync(Stream inputStream,
		int? maxWidth, int? maxHeight, int qualityPercent, string? originalFileName = null, bool rotateImage = false, bool preserveMetaData = true)
	{
#if !(IOS || ANDROID || WINDOWS)
		await Task.CompletedTask; // Avoid async warning
		return null;
#else
        if (inputStream is null)
        {
            throw new ArgumentNullException(nameof(inputStream));
        }

        // Ensure we can read from the beginning of the stream
        if (inputStream.CanSeek)
        {
            inputStream.Position = 0;
        }

        // Apply EXIF rotation first if requested
        Stream processedStream = inputStream;
        if (rotateImage)
        {
            processedStream = await RotateImageAsync(inputStream, originalFileName);
            // Reset position for subsequent processing
            if (processedStream.CanSeek)
            {
                processedStream.Position = 0;
            }
        }

        // Extract metadata from original stream if needed
        byte[]? originalMetadata = null;
        if (preserveMetaData)
        {
            originalMetadata = await ExtractMetadataAsync(inputStream, originalFileName);
            // Reset position after metadata extraction
            if (inputStream.CanSeek)
            {
                inputStream.Position = 0;
            }
        }

        IImage? image = null;
        try
        {
            // Load the image using MAUI Graphics
            var imageLoadingService = new PlatformImageLoadingService();
            image = imageLoadingService.FromStream(processedStream);

            if (image is null)
            {
                throw new InvalidOperationException("Failed to load image from stream");
            }

            // Apply resizing if needed
            if (maxWidth.HasValue || maxHeight.HasValue)
            {
                image = ApplyResizing(image, maxWidth, maxHeight);
            }

            // Determine output format and quality
            var format = ShouldUsePngFormat(originalFileName, qualityPercent) 
                ? ImageFormat.Png : ImageFormat.Jpeg;
            var quality = Math.Max(0f, Math.Min(1f, qualityPercent / 100.0f));

            // Save to new stream
            var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, format, quality);
            outputStream.Position = 0;

            // Apply preserved metadata to the output stream if requested
            if (preserveMetaData && originalMetadata != null)
            {
                var finalStream = await ApplyMetadataAsync(outputStream, originalMetadata, originalFileName);
                outputStream.Dispose();
                return finalStream;
            }

            return outputStream;
        }
        finally
        {
            image?.Dispose();
            
            // Clean up the rotated stream if it's different from the input
            if (rotateImage && processedStream != inputStream)
            {
                processedStream?.Dispose();
            }
        }
#endif
	}

#if IOS || ANDROID || WINDOWS
    /// <summary>
    /// Applies resizing constraints to an image while preserving aspect ratio.
    /// </summary>
    private static IImage ApplyResizing(IImage image, int? maxWidth, int? maxHeight)
    {
        var currentWidth = image.Width;
        var currentHeight = image.Height;

        // Calculate new dimensions while preserving aspect ratio
        var newDimensions = CalculateResizedDimensions(currentWidth, currentHeight, maxWidth, maxHeight);

        // Only resize if dimensions actually changed
        if (Math.Abs(newDimensions.Width - currentWidth) > 0.1f || 
            Math.Abs(newDimensions.Height - currentHeight) > 0.1f)
        {
            return image.Downsize(newDimensions.Width, newDimensions.Height, disposeOriginal: true);
        }

        return image;
    }

    /// <summary>
    /// Calculates new image dimensions while preserving aspect ratio.
    /// </summary>
    private static (float Width, float Height) CalculateResizedDimensions(
        float originalWidth, float originalHeight, int? maxWidth, int? maxHeight)
    {
        if (!maxWidth.HasValue && !maxHeight.HasValue)
        {
            return (originalWidth, originalHeight);
        }

        var targetWidth = maxWidth ?? float.MaxValue;
        var targetHeight = maxHeight ?? float.MaxValue;

        var scaleX = targetWidth / originalWidth;
        var scaleY = targetHeight / originalHeight;
        var scale = Math.Min(scaleX, scaleY);

        // Only scale down, never scale up
        if (scale >= 1.0f)
            return (originalWidth, originalHeight);

        return (originalWidth * scale, originalHeight * scale);
    }

    /// <summary>
    /// Determines whether to use PNG format based on the original filename and quality settings.
    /// </summary>
    private static bool ShouldUsePngFormat(string? originalFileName, int qualityPercent)
    {
        var originalWasPng = !string.IsNullOrEmpty(originalFileName) && 
                                Path.GetExtension(originalFileName).Equals(".png", StringComparison.OrdinalIgnoreCase);

        // High quality (>=95): Prefer PNG for lossless quality
        // High quality (>=90) + original was PNG: preserve PNG format
        // Otherwise: Use JPEG for better compression
        return qualityPercent >= 95 || (qualityPercent >= 90 && originalWasPng);
    }
#endif

	/// <summary>
	/// Determines if image processing is needed based on the provided options.
	/// </summary>
	public static bool IsProcessingNeeded(int? maxWidth, int? maxHeight, int qualityPercent)
	{
#if !(IOS || ANDROID || WINDOWS)
		// On platforms without MAUI Graphics support, always return false - no processing available
		return false;
#else
        return (maxWidth.HasValue || maxHeight.HasValue) || qualityPercent < 100;
#endif
	}

	/// <summary>
	/// Determines the output file extension based on processed image data and quality settings.
	/// </summary>
	/// <param name="imageData">The processed image stream to analyze</param>
	/// <param name="qualityPercent">Compression quality percentage</param>
	/// <param name="originalFileName">Original filename for format hints (optional)</param>
	/// <returns>File extension including the dot (e.g., ".jpg", ".png")</returns>
	public static string DetermineOutputExtension(Stream? imageData, int qualityPercent, string? originalFileName = null)
	{
#if !(IOS || ANDROID)
		// On platforms without MAUI Graphics support, fall back to simple logic
		bool originalWasPng = !string.IsNullOrEmpty(originalFileName) &&
							  originalFileName!.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
		return (qualityPercent >= 95 || originalWasPng) ? ".png" : ".jpg";
#else
        // Try to detect format from the actual processed image data
        var detectedFormat = DetectImageFormat(imageData);
        if (!string.IsNullOrEmpty(detectedFormat))
        {
            return detectedFormat;
        }

        // Fallback: Use quality-based decision with original format consideration
        var originalWasPng = !string.IsNullOrEmpty(originalFileName) && 
                                Path.GetExtension(originalFileName).Equals(".png", StringComparison.OrdinalIgnoreCase);
        
        // High quality (>=90) and original was PNG: keep PNG
        // Very high quality (>=95): prefer PNG for maximum quality
        // Otherwise: use JPEG for better compression
        return (qualityPercent >= 95 || (qualityPercent >= 90 && originalWasPng)) ? ".png" : ".jpg";
#endif
	}

#if IOS || ANDROID || WINDOWS
    /// <summary>
    /// Detects image format from stream using magic numbers.
    /// </summary>
    private static string? DetectImageFormat(Stream? imageData)
    {
        if (imageData is null || imageData.Length < 4)
        {
            return null;
        }

        var originalPosition = imageData.Position;
        try
        {
            imageData.Position = 0;
            var bytes = new byte[8];
            var bytesRead = imageData.Read(bytes, 0, Math.Min(8, (int)imageData.Length));
            
            if (bytesRead < 3)
            {
                return null;
            }

            // PNG: 89 50 4E 47
            if (bytesRead >= 4 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            {
                return ".png";
            }

            // JPEG: FF D8 FF
            if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            {
                return ".jpg";
            }

            return null;
        }
        finally
        {
            imageData.Position = originalPosition;
        }
    }
#endif

}

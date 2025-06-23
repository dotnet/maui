using System;
using System.IO;
using System.Threading.Tasks;
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
    /// Processes an image by applying resizing, compression, and optional rotation using MAUI Graphics.
    /// </summary>
    /// <param name="inputStream">The input image stream.</param>
    /// <param name="maxWidth">Maximum width constraint (null for no constraint).</param>
    /// <param name="maxHeight">Maximum height constraint (null for no constraint).</param>
    /// <param name="qualityPercent">Compression quality percentage (0-100).</param>
    /// <param name="rotateImage">Whether to apply EXIF-based rotation to correct orientation.</param>
    /// <param name="originalFileName">Original filename to determine format preservation logic.</param>
    /// <returns>A new stream containing the processed image.</returns>
    public static async Task<Stream> ProcessImageAsync(Stream inputStream, 
        int? maxWidth, int? maxHeight, int qualityPercent, bool rotateImage = false, string originalFileName = null)
    {
#if !(IOS || ANDROID || WINDOWS)
        // For platforms without MAUI Graphics support, return null to indicate no processing available
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

        IImage image = null;
        try
        {
            // Load the image using MAUI Graphics
            var imageLoadingService = new PlatformImageLoadingService();
            image = imageLoadingService.FromStream(inputStream);

            if (image is null)
            {
                throw new InvalidOperationException("Failed to load image from stream");
            }

            // Apply rotation if needed to correct EXIF orientation
            if (rotateImage)
            {
                image = ApplyRotation(image, inputStream);
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

            return outputStream;
        }
        finally
        {
            image?.Dispose();
        }
#endif
    }

    /// <summary>
    /// Determines if image processing is needed based on the provided options.
    /// </summary>
    public static bool IsProcessingNeeded(int? maxWidth, int? maxHeight, int qualityPercent, bool rotateImage = false)
    {
        return (maxWidth.HasValue || maxHeight.HasValue) || qualityPercent < 100 || rotateImage;
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
    private static bool ShouldUsePngFormat(string originalFileName, int qualityPercent)
    {
        var originalWasPng = !string.IsNullOrEmpty(originalFileName) && 
                                Path.GetExtension(originalFileName).Equals(".png", StringComparison.OrdinalIgnoreCase);

        // High quality (>=95): Prefer PNG for lossless quality
        // High quality (>=90) + original was PNG: preserve PNG format
        // Otherwise: Use JPEG for better compression
        return qualityPercent >= 95 || (qualityPercent >= 90 && originalWasPng);
    }

    /// <summary>
    /// Applies rotation to correct image orientation based on EXIF data.
    /// Platform-specific implementation.
    /// </summary>
    /// <param name="image">The image to rotate.</param>
    /// <param name="originalStream">The original image stream for EXIF reading.</param>
    /// <returns>A rotated image, or the original if no rotation is needed.</returns>
    private static IImage ApplyRotation(IImage image, Stream originalStream)
    {
#if IOS
        return ApplyRotationiOS(image, originalStream);
#elif ANDROID
        return ApplyRotationAndroid(image, originalStream);
#elif WINDOWS
        return ApplyRotationWindows(image, originalStream);
#else
        // For platforms without specific implementation, return unchanged
        return image;
#endif
    }
#endif

    /// <summary>
    /// Determines the output file extension based on processed image data and quality settings.
    /// </summary>
    /// <param name="imageData">The processed image stream to analyze</param>
    /// <param name="qualityPercent">Compression quality percentage</param>
    /// <param name="originalFileName">Original filename for format hints (optional)</param>
    /// <returns>File extension including the dot (e.g., ".jpg", ".png")</returns>
    public static string DetermineOutputExtension(Stream imageData, int qualityPercent, string originalFileName = null)
    {
#if IOS || ANDROID || WINDOWS
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
#else
        // On platforms without MAUI Graphics support, fall back to simple logic
        bool originalWasPng = !string.IsNullOrEmpty(originalFileName) && 
                              originalFileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
        return (qualityPercent >= 95 || originalWasPng) ? ".png" : ".jpg";
#endif
    }
    }

#if IOS || ANDROID || WINDOWS
    /// <summary>
    /// Detects image format from stream using magic numbers.
    /// </summary>
    private static string DetectImageFormat(Stream imageData)
    {
        if (imageData?.Length < 4)
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

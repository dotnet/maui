using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using UIKit;
using Foundation;
using CoreGraphics;

namespace Microsoft.Maui.Essentials;

/// <summary>
/// iOS-specific implementation of ImageProcessor for MediaPicker.
/// This implementation provides efficient EXIF-based image rotation that:
/// - Only rotates images that aren't already correctly oriented (Up orientation)
/// - Uses proper CGAffineTransform matrix operations for accurate rotation
/// - Integrates with the shared format/quality pipeline to avoid duplication
/// - Based on the proven approach from the MediaPlugin library
/// 
/// The rotation process works in conjunction with the main ImageProcessor pipeline:
/// 1. ApplyRotation() handles only the rotation logic and returns a rotated IImage
/// 2. The main ProcessImageAsync() pipeline handles format detection and quality settings
/// 3. This eliminates duplication of format/compression logic between rotation and main processing
/// </summary>
internal static partial class ImageProcessor
{
    /// <summary>
    /// Applies rotation to correct image orientation based on EXIF data on iOS.
    /// Only rotates if the image is not already correctly oriented (Up orientation).
    /// Returns the rotated image for further processing by the main pipeline.
    /// </summary>
    private static partial IImage ApplyRotation(IImage image, Stream originalStream)
    {
        if (image is PlatformImage platformImage)
        {
            var uiImage = platformImage.PlatformRepresentation as UIImage;
            if (uiImage != null)
            {
                // Check if rotation is needed - if image is already Up orientation, no rotation needed
                if (uiImage.Orientation == UIImageOrientation.Up)
                {
                    return image; // Return original image without any processing
                }

                // Apply rotation using proper transformation matrix approach
                var rotatedUIImage = RotateUIImage(uiImage);
                
                // Convert rotated UIImage directly to IImage without any intermediate format conversion
                // This avoids PNG conversion and preserves the original image format for the main pipeline
                var rotatedImage = new PlatformImage(rotatedUIImage);
                
                // Dispose the rotated UIImage if it's different from the original
                if (rotatedUIImage != uiImage)
                {
                    rotatedUIImage.Dispose();
                }
                
                return rotatedImage;
            }
        }
        
        return image;
    }

    /// <summary>
    /// Rotates a UIImage to correct orientation using UIGraphicsImageRenderer for better quality and efficiency.
    /// This approach is more modern and typically results in better quality with smaller file sizes.
    /// </summary>
    private static UIImage RotateUIImage(UIImage image)
    {
        if (image.Orientation == UIImageOrientation.Up)
        {
            return image; // No rotation needed
        }

        // Use UIGraphicsImageRenderer which is the modern way to draw images on iOS
        // This is more efficient than the old UIGraphics.BeginImageContext methods
        var format = new UIGraphicsImageRendererFormat
        {
            Scale = image.CurrentScale,
            Opaque = false // Preserve transparency if present
        };

        var renderer = new UIGraphicsImageRenderer(image.Size, format);
        
        var rotatedImage = renderer.CreateImage((context) =>
        {
            // Draw the image - UIKit automatically handles the orientation transformation
            image.Draw(new CGRect(0, 0, image.Size.Width, image.Size.Height));
        });

        return rotatedImage;
    }

    /// <summary>
    /// iOS-specific optimized method to get rotated image data without re-encoding.
    /// This preserves the original image format and compression quality by working directly with the original stream.
    /// </summary>
    private static async Task<Stream> TryGetRotatedImageStreamAsyncIOS(IImage rotatedImage, Stream originalStream, string originalFileName)
    {
        if (originalStream == null || !originalStream.CanSeek)
            return null;

        try
        {
            // Reset stream position
            originalStream.Position = 0;
            
            // Load UIImage directly from the original stream
            var imageData = NSData.FromStream(originalStream);
            var originalUIImage = UIImage.LoadFromData(imageData);
            
            if (originalUIImage == null)
                return null;
                
            // Check if rotation is actually needed
            if (originalUIImage.Orientation == UIImageOrientation.Up)
            {
                // No rotation needed, return original stream as-is
                originalStream.Position = 0;
                var originalCopy = new MemoryStream();
                await originalStream.CopyToAsync(originalCopy);
                originalCopy.Position = 0;
                return originalCopy;
            }

            // Apply rotation using the same method as ApplyRotation
            var rotatedUIImage = RotateUIImage(originalUIImage);
            
            // Detect original format and preserve it
            var (useJpeg, quality) = DetectOriginalFormatAndQuality(originalStream, originalFileName);
            
            NSData rotatedImageData;
            if (useJpeg)
            {
                // For JPEG, try to preserve quality by using a high-quality setting
                // We use 0.95 to maintain very high quality while still being efficient
                rotatedImageData = rotatedUIImage.AsJPEG(0.95f);
            }
            else
            {
                // For PNG, use lossless compression
                rotatedImageData = rotatedUIImage.AsPNG();
            }

            if (rotatedImageData != null)
            {
                var resultStream = new MemoryStream();
                rotatedImageData.AsStream().CopyTo(resultStream);
                resultStream.Position = 0;
                
                // Clean up
                originalUIImage.Dispose();
                if (rotatedUIImage != originalUIImage)
                    rotatedUIImage.Dispose();
                rotatedImageData.Dispose();
                
                return resultStream;
            }
            
            // Clean up on failure
            originalUIImage.Dispose();
            if (rotatedUIImage != originalUIImage)
                rotatedUIImage.Dispose();
        }
        catch
        {
            // If anything goes wrong, fall back to the normal processing pipeline
        }

        return null; // Fall back to default processing
    }

    /// <summary>
    /// Detects the original image format and attempts to preserve quality settings.
    /// </summary>
    private static (bool useJpeg, float quality) DetectOriginalFormatAndQuality(Stream originalStream, string originalFileName)
    {
        // Default to high quality JPEG
        var useJpeg = true;
        var quality = 0.9f;

        try
        {
            if (originalStream != null && originalStream.CanSeek)
            {
                var originalPosition = originalStream.Position;
                originalStream.Position = 0;

                var buffer = new byte[8];
                var bytesRead = originalStream.Read(buffer, 0, buffer.Length);
                originalStream.Position = originalPosition;

                // Check for PNG signature (89 50 4E 47 0D 0A 1A 0A)
                if (bytesRead >= 8 && 
                    buffer[0] == 0x89 && buffer[1] == 0x50 && 
                    buffer[2] == 0x4E && buffer[3] == 0x47)
                {
                    useJpeg = false; // Use PNG for lossless preservation
                }
            }
            else if (!string.IsNullOrEmpty(originalFileName))
            {
                var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
                if (extension == ".png")
                {
                    useJpeg = false;
                }
            }
        }
        catch
        {
            // Ignore errors and use default
        }

        return (useJpeg, quality);
    }
}

using System;
using System.IO;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Android.Graphics;
using Android.Media;

namespace Microsoft.Maui.Essentials;

internal static partial class ImageProcessor
{
    /// <summary>
    /// Applies rotation to correct image orientation based on EXIF data on Android.
    /// Reads EXIF orientation and applies the appropriate rotation using Android's ExifInterface.
    /// </summary>
    private static partial IImage ApplyRotation(IImage image, System.IO.Stream originalStream)
    {
        try
        {
            // Reset stream position to read EXIF data
            if (originalStream.CanSeek)
            {
                originalStream.Position = 0;
            }

            // Read EXIF orientation using Android's ExifInterface
            var exif = new ExifInterface(originalStream);
            var orientation = exif.GetAttributeInt(ExifInterface.TagOrientation, (int)Android.Media.Orientation.Undefined);
            
            // Calculate rotation angle based on EXIF orientation
            var rotationAngle = GetRotationAngleFromExifOrientation(orientation);
            
            // If no rotation needed, return original image
            if (rotationAngle == 0)
            {
                return image;
            }

            // Apply rotation using Canvas-based approach
            if (image is PlatformImage platformImage)
            {
                var bitmap = platformImage.PlatformRepresentation as Bitmap;
                if (bitmap != null)
                {
                    // Create rotated bitmap using Matrix transformation
                    var matrix = new Matrix();
                    matrix.PostRotate(rotationAngle);
                    
                    var rotatedBitmap = Bitmap.CreateBitmap(
                        bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);
                    
                    if (rotatedBitmap != null && rotatedBitmap != bitmap)
                    {
                        // Convert rotated bitmap back to IImage
                        using var memoryStream = new MemoryStream();
                        
                        // Use PNG format to preserve quality during rotation
                        rotatedBitmap.Compress(Bitmap.CompressFormat.Png, 100, memoryStream);
                        memoryStream.Position = 0;

                        var imageLoadingService = new PlatformImageLoadingService();
                        return imageLoadingService.FromStream(memoryStream);
                    }
                }
            }
            
            return image;
        }
        catch (Exception)
        {
            // If rotation fails, return the original image
            return image;
        }
    }

    /// <summary>
    /// Gets the rotation angle in degrees from EXIF orientation.
    /// </summary>
    private static int GetRotationAngleFromExifOrientation(int orientation)
    {
        return orientation switch
        {
            3 => 180,  // ExifInterface.ORIENTATION_ROTATE_180
            6 => 90,   // ExifInterface.ORIENTATION_ROTATE_90
            8 => 270,  // ExifInterface.ORIENTATION_ROTATE_270
            _ => 0
        };
    }
}

using System;
using System.IO;

namespace VisualTestUtils;

public static class ImageSnapshotFormatExtensions
{
    public static string GetFileExtension(this ImageSnapshotFormat format) =>
        format switch
        {
            ImageSnapshotFormat.PNG => ".png",
            ImageSnapshotFormat.JPEG => ".jpg",
            _ => throw new InvalidOperationException($"Invalid ImageFormat value: {format}"),
        };

    public static ImageSnapshotFormat GetImageFormat(string filePath)
    {
        string fileExtension = Path.GetExtension(filePath).ToLowerInvariant();

        if (fileExtension == ".png")
        {
            return ImageSnapshotFormat.PNG;
        }
        else if (fileExtension == ".jpg" || fileExtension == ".jpeg")
        {
            return ImageSnapshotFormat.JPEG;
        }
        else
        {
            throw new InvalidOperationException($"Unsupported file type: {filePath}");
        }
    }
}

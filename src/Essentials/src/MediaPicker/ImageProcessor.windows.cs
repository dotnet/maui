#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace Microsoft.Maui.Essentials;

internal static partial class ImageProcessor
{
	public static partial async Task<Stream> RotateImageAsync(Stream inputStream, string? originalFileName)
	{
		try
		{
			// Convert Stream to IRandomAccessStream
			var randomAccessStream = new InMemoryRandomAccessStream();
			await inputStream.CopyToAsync(randomAccessStream.AsStreamForWrite());
			randomAccessStream.Seek(0);

			// Create a decoder from the input stream
			var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
			
			// Check if rotation is needed
			var orientation = GetImageOrientation(decoder);
			if (orientation == BitmapRotation.None)
			{
				inputStream.Position = 0;
				return inputStream;
			}

			// Create output stream
			var outputStream = new InMemoryRandomAccessStream();

			// Create encoder
			var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, outputStream);

			// Set the transform with rotation
			encoder.BitmapTransform.Rotation = orientation;

			// Set pixel data
			var pixelData = await decoder.GetPixelDataAsync();
			encoder.SetPixelData(
				decoder.BitmapPixelFormat,
				decoder.BitmapAlphaMode,
				decoder.OrientedPixelWidth,
				decoder.OrientedPixelHeight,
				decoder.DpiX,
				decoder.DpiY,
				pixelData.DetachPixelData());

			// Finalize encoding
			await encoder.FlushAsync();

			// Return as regular stream
			var resultStream = outputStream.AsStreamForRead();
			return resultStream;
		}
		catch
		{
			// If anything fails, return the original stream
			inputStream.Position = 0;
			return inputStream;
		}
	}

	static BitmapRotation GetImageOrientation(BitmapDecoder decoder)
	{
		try
		{
			// Try to get the EXIF orientation
			var properties = decoder.BitmapProperties;
			var orientationProperty = properties.GetPropertiesAsync(new[] { "System.Photo.Orientation" }).GetAwaiter().GetResult();
			
			if (orientationProperty.TryGetValue("System.Photo.Orientation", out var orientationValue) && 
				orientationValue.Value is ushort orientation)
			{
				return orientation switch
				{
					3 => BitmapRotation.Clockwise180Degrees,
					6 => BitmapRotation.Clockwise90Degrees,
					8 => BitmapRotation.Clockwise270Degrees,
					_ => BitmapRotation.None
				};
			}
		}
		catch
		{
			// If we can't read EXIF data, assume no rotation needed
		}

		return BitmapRotation.None;
	}
}

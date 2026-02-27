#nullable enable
using System;
using System.Collections.Generic;
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
			var orientation = await GetImageOrientation(decoder);
			if (orientation == BitmapRotation.None)
			{
				inputStream.Position = 0;
				return inputStream;
			}

			// Create output stream
			var outputStream = new InMemoryRandomAccessStream();

			// Create encoder
			Guid encoderId = BitmapEncoder.JpegEncoderId;

			// If we don't have the filename, assume Jpeg
			if (Path.GetExtension(originalFileName ?? string.Empty).Equals(".png", StringComparison.OrdinalIgnoreCase))
			{
				encoderId = BitmapEncoder.PngEncoderId;
			}

			var encoder = await BitmapEncoder.CreateAsync(encoderId, outputStream);

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

	static async Task<BitmapRotation> GetImageOrientation(BitmapDecoder decoder)
	{
		try
		{
			// Try to get the EXIF orientation
			var properties = decoder.BitmapProperties;
			var orientationProperty = await properties.GetPropertiesAsync(new[] { "System.Photo.Orientation" });
			
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

	public static partial async Task<byte[]?> ExtractMetadataAsync(Stream inputStream, string? originalFileName)
	{
		if (inputStream == null)
			return null;

		try
		{
			// Convert Stream to IRandomAccessStream
			var randomAccessStream = new InMemoryRandomAccessStream();
			await inputStream.CopyToAsync(randomAccessStream.AsStreamForWrite());
			randomAccessStream.Seek(0);

			// Create a decoder from the input stream
			var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);

			// Get all properties
			var properties = await decoder.BitmapProperties.GetPropertiesAsync(Array.Empty<string>());
			
			// Serialize properties to a simple format
			var metadataList = new List<string>();
			foreach (var prop in properties)
			{
				if (prop.Value != null && prop.Value.Value != null)
				{
					metadataList.Add($"{prop.Key}={prop.Value.Value}");
				}
			}

			var metadataString = string.Join("\n", metadataList);
			return System.Text.Encoding.UTF8.GetBytes(metadataString);
		}
		catch
		{
			return null;
		}
	}

	public static partial async Task<Stream> ApplyMetadataAsync(Stream processedStream, byte[] metadata, string? originalFileName)
	{
		if (processedStream == null || metadata == null || metadata.Length == 0)
			return await Task.FromResult(processedStream) ?? new MemoryStream();

		try
		{
			// For now, Windows doesn't have a simple way to reapply metadata to processed images
			// The Windows Imaging Component (WIC) makes this complex, so we'll return the processed stream as-is
			// In the future, this could be enhanced with more sophisticated metadata handling
			return await Task.FromResult(processedStream);
		}
		catch
		{
			return await Task.FromResult(processedStream);
		}
	}
}

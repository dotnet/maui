#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Media;
using Path = System.IO.Path;
using Stream = System.IO.Stream;

namespace Microsoft.Maui.Essentials;

internal static partial class ImageProcessor
{
	public static partial async Task<Stream> RotateImageAsync(Stream inputStream, string? originalFileName)
	{
		if (inputStream is null)
		{
			return new MemoryStream();
		}

		// Reset stream position
		if (inputStream.CanSeek)
		{
			inputStream.Position = 0;
		}

		// Read the input stream into a byte array
		byte[] bytes;
		using (var memoryStream = new MemoryStream())
		{
			await inputStream.CopyToAsync(memoryStream);
			bytes = memoryStream.ToArray();
		}

		try
		{
			// Load the bitmap from bytes
			using var originalBitmap = await Task.Run(() => BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length));
			if (originalBitmap is null)
			{
				return new MemoryStream(bytes);
			}

			// Get EXIF orientation
			int orientation = GetExifOrientation(bytes);

			// If orientation is normal, return original
			if (orientation == 1)
			{
				return new MemoryStream(bytes);
			}

			// Apply EXIF orientation correction using SetRotate(0) to preserve original EXIF behavior
			Bitmap? rotatedBitmap = ApplyExifOrientation(originalBitmap);
			if (rotatedBitmap is null)
			{
				return new MemoryStream(bytes);
			}

			// Clean up the original bitmap if we created a new one
			if (rotatedBitmap != originalBitmap)
			{
				originalBitmap.Recycle();
			}

			// Convert the rotated bitmap back to a stream
			var resultStream = new MemoryStream();
			bool usePng = !string.IsNullOrEmpty(originalFileName) &&
						  Path.GetExtension(originalFileName).Equals(".png", StringComparison.OrdinalIgnoreCase);

			var compressResult = await Task.Run(() =>
			{
				try
				{
					if (usePng)
					{
						return rotatedBitmap.Compress(Bitmap.CompressFormat.Png!, 100, resultStream);
					}
					else
					{
						return rotatedBitmap.Compress(Bitmap.CompressFormat.Jpeg!, 100, resultStream);
					}
				}
				catch (Exception ex)
				{
					System.Console.WriteLine($"Compression error: {ex}");
					return false;
				}
				finally
				{
					rotatedBitmap?.Recycle();
				}
			});

			if (!compressResult)
			{
				return new MemoryStream(bytes);
			}

			resultStream.Position = 0;
			return resultStream;
		}
		catch (Exception ex)
		{
			System.Console.WriteLine($"Exception in RotateImageAsync: {ex}");
			return new MemoryStream(bytes);
		}
	}

	/// <summary>
	/// Extract EXIF orientation from image bytes
	/// </summary>
	private static int GetExifOrientation(byte[] imageBytes)
	{
		try
		{
			// Create a temporary file to read EXIF data
			var tempFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.jpg");
			using (var fileStream = File.Create(tempFileName))
			{
				fileStream.Write(imageBytes, 0, imageBytes.Length);
			}

			var exif = new ExifInterface(tempFileName);
			int orientation = exif.GetAttributeInt(ExifInterface.TagOrientation, 1);

			// Clean up temp file
			try
			{
				File.Delete(tempFileName);
			}
			catch
			{
				// Ignore cleanup failures
			}

			return orientation;
		}
		catch
		{
			return 1; // Default to normal orientation
		}
	}

	/// <summary>
	/// Apply EXIF orientation correction by preserving original EXIF behavior
	/// </summary>
	private static Bitmap? ApplyExifOrientation(Bitmap bitmap)
	{
		try
		{
			// Use SetRotate(0) to preserve original EXIF orientation behavior
			var matrix = new Matrix();
			matrix.SetRotate(0);
			return Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);
		}
		catch (Exception ex)
		{
			System.Console.WriteLine($"Error applying EXIF orientation: {ex}");
			return bitmap;
		}
	}

	public static partial async Task<byte[]?> ExtractMetadataAsync(Stream inputStream, string? originalFileName)
	{
		if (inputStream == null)
			return null;

		try
		{
			// Reset stream position
			if (inputStream.CanSeek)
				inputStream.Position = 0;

			// Read stream into byte array
			byte[] bytes;
			using (var memoryStream = new MemoryStream())
			{
				await inputStream.CopyToAsync(memoryStream);
				bytes = memoryStream.ToArray();
			}

			// Create temporary file to extract EXIF data
			var tempFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.jpg");
			using (var fileStream = File.Create(tempFileName))
			{
				fileStream.Write(bytes, 0, bytes.Length);
			}

			// Extract all EXIF attributes
			var exif = new ExifInterface(tempFileName);
			var metadataList = new List<string>();

			// Extract common EXIF tags
			var tags = new string[]
			{
				ExifInterface.TagArtist,
				ExifInterface.TagCopyright,
				ExifInterface.TagDatetime,
				ExifInterface.TagImageDescription,
				ExifInterface.TagMake,
				ExifInterface.TagModel,
				ExifInterface.TagOrientation,
				ExifInterface.TagSoftware,
				ExifInterface.TagGpsLatitude,
				ExifInterface.TagGpsLongitude,
				ExifInterface.TagGpsAltitude,
				ExifInterface.TagExposureTime,
				ExifInterface.TagFNumber,
				ExifInterface.TagIso,
				ExifInterface.TagWhiteBalance,
				ExifInterface.TagFlash,
				ExifInterface.TagFocalLength
			};

			foreach (var tag in tags)
			{
				var value = exif.GetAttribute(tag);
				if (!string.IsNullOrEmpty(value))
				{
					metadataList.Add($"{tag}={value}");
				}
			}

			// Serialize metadata to simple string format
			var metadataString = string.Join("\n", metadataList);
			var metadataBytes = System.Text.Encoding.UTF8.GetBytes(metadataString);

			// Clean up temp file
			try
			{
				File.Delete(tempFileName);
			}
			catch
			{
				// Ignore cleanup failures
			}

			return metadataBytes;
		}
		catch
		{
			return null;
		}
	}

	public static partial async Task<Stream> ApplyMetadataAsync(Stream processedStream, byte[] metadata, string? originalFileName)
	{
		if (processedStream == null || metadata == null || metadata.Length == 0)
			return processedStream ?? new MemoryStream();

		try
		{
			// Reset stream position
			if (processedStream.CanSeek)
				processedStream.Position = 0;

			// Read processed stream into byte array
			byte[] bytes;
			using (var memoryStream = new MemoryStream())
			{
				await processedStream.CopyToAsync(memoryStream);
				bytes = memoryStream.ToArray();
			}

			// Deserialize metadata
			var metadataString = System.Text.Encoding.UTF8.GetString(metadata);
			var metadataLines = metadataString.Split('\n', StringSplitOptions.RemoveEmptyEntries);

			var metadataDict = new Dictionary<string, string>();
			foreach (var line in metadataLines)
			{
				var parts = line.Split('=', 2);
				if (parts.Length == 2)
				{
					metadataDict[parts[0]] = parts[1];
				}
			}

			if (metadataDict.Count == 0)
				return new MemoryStream(bytes);

			// Create temporary file to apply EXIF data
			var tempFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.jpg");
			using (var fileStream = File.Create(tempFileName))
			{
				fileStream.Write(bytes, 0, bytes.Length);
			}

			// Apply EXIF data
			var exif = new ExifInterface(tempFileName);
			foreach (var kvp in metadataDict)
			{
				try
				{
					exif.SetAttribute(kvp.Key, kvp.Value);
				}
				catch
				{
					// Skip attributes that can't be set
				}
			}
			exif.SaveAttributes();

			// Read back the file with applied metadata
			var resultBytes = File.ReadAllBytes(tempFileName);

			// Clean up temp file
			try
			{
				File.Delete(tempFileName);
			}
			catch
			{
				// Ignore cleanup failures
			}

			return new MemoryStream(resultBytes);
		}
		catch
		{
			// If metadata application fails, return original processed stream
			if (processedStream.CanSeek)
				processedStream.Position = 0;
			return processedStream;
		}
	}
}

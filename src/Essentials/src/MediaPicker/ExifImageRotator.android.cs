#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Media;
using Microsoft.Maui.ApplicationModel;
using Path = System.IO.Path;
using Stream = System.IO.Stream;

namespace Microsoft.Maui.Media
{
	internal static partial class ExifImageRotator
	{
		public static partial async Task<Stream> RotateImageAsync(Stream inputStream, string originalFileName)
		{
			if (inputStream == null)
				return new MemoryStream();

			// Reset stream position
			if (inputStream.CanSeek)
				inputStream.Position = 0;

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
				var originalBitmap = await Task.Run(() => BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length));
				if (originalBitmap == null)
					return new MemoryStream(bytes);

				// Get EXIF orientation
				int orientation = GetExifOrientation(bytes);
				
				// If orientation is normal, return original
				if (orientation == 1)
				{
					return new MemoryStream(bytes);
				}

				// Apply EXIF orientation correction using SetRotate(0) to preserve original EXIF behavior
				Bitmap? rotatedBitmap = ApplyExifOrientation(originalBitmap);
				if (rotatedBitmap == null)
				{
					return new MemoryStream(bytes);
				}

				// Clean up the original bitmap if we created a new one
				if (rotatedBitmap != originalBitmap)
					originalBitmap.Recycle();

				// Convert the rotated bitmap back to a stream
				var resultStream = new MemoryStream();
				bool usePng = !string.IsNullOrEmpty(originalFileName) && 
							  Path.GetExtension(originalFileName).ToLowerInvariant() == ".png";

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
					return new MemoryStream(bytes);

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
				using (var fileStream = System.IO.File.Create(tempFileName))
				{
					fileStream.Write(imageBytes, 0, imageBytes.Length);
				}

				var exif = new ExifInterface(tempFileName);
				int orientation = exif.GetAttributeInt(ExifInterface.TagOrientation, 1);
				
				// Clean up temp file
				try
				{
					System.IO.File.Delete(tempFileName);
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
	}
}

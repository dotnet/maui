#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media.Imaging;

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

			try
			{
				// Create an in-memory random access stream from the input
				using var memoryStream = new MemoryStream();
				await inputStream.CopyToAsync(memoryStream);
				var buffer = memoryStream.ToArray();

				// Create a random access stream from the buffer
				using var randomAccessStream = new InMemoryRandomAccessStream();
				using (var writer = new DataWriter(randomAccessStream.GetOutputStreamAt(0)))
				{
					writer.WriteBytes(buffer);
					await writer.StoreAsync();
					await writer.FlushAsync();
				}

				// Create a decoder from the stream
				var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);

				// Check the orientation from the EXIF data
				var orientation = decoder.OrientedPixelWidth > decoder.OrientedPixelHeight 
					? decoder.OrientedPixelWidth > decoder.PixelWidth 
						? BitmapRotation.Clockwise90Degrees 
						: BitmapRotation.None
					: decoder.OrientedPixelHeight > decoder.PixelHeight 
						? BitmapRotation.Clockwise270Degrees 
						: BitmapRotation.None;

				// If no rotation needed, return the original stream
				if (orientation == BitmapRotation.None)
				{
					return new MemoryStream(buffer);
				}

				// Get the software bitmap
				var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

				// Create a new in-memory stream for the result
				var resultStream = new InMemoryRandomAccessStream();

				// Determine the encoder based on the file extension
				Guid encoderId;
				if (!string.IsNullOrEmpty(originalFileName) && 
					originalFileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
				{
					encoderId = BitmapEncoder.PngEncoderId;
				}
				else
				{
					encoderId = BitmapEncoder.JpegEncoderId;
				}

				// Create an encoder for the result stream
				var encoder = await BitmapEncoder.CreateAsync(encoderId, resultStream);

				// Set the software bitmap and apply the rotation
				encoder.SetSoftwareBitmap(softwareBitmap);
				encoder.BitmapTransform.Rotation = orientation;

				// Apply the changes
				await encoder.FlushAsync();

				// Return the result as a managed stream
				var outputStream = new MemoryStream();
				var outputRandomStream = resultStream.AsStream();
				outputRandomStream.Position = 0;
				await outputRandomStream.CopyToAsync(outputStream);
				outputStream.Position = 0;

				return outputStream;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Exception in RotateImageAsync: {ex}");
				
				// If anything goes wrong, return the input stream (reset position first)
				if (inputStream.CanSeek)
					inputStream.Position = 0;
                
				// Copy to memory stream to ensure we return a new stream
				var fallbackStream = new MemoryStream();
				await inputStream.CopyToAsync(fallbackStream);
				fallbackStream.Position = 0;
                
				return fallbackStream;
			}
		}
	}
}

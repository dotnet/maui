using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests.Shared
{
	[Category("ImageProcessor")]
	public class ImageProcessor_Tests
	{
		const uint Width = 1600;
		const uint Height = 960;

		// Clockwise from top left: red, green, yellow, blue. The unequal dimensions
		// and four distinct quadrants distinguish every quarter turn.
		static readonly byte[][] Colors =
		{
			new byte[] { 255, 0, 0 },
			new byte[] { 0, 255, 0 },
			new byte[] { 0, 0, 255 },
			new byte[] { 255, 255, 0 }
		};

		[Theory]
		[InlineData(".jpg", 1, 1600, 960, 0, 1, 2, 3)]
		[InlineData(".jpg", 6, 960, 1600, 2, 0, 3, 1)]
		[InlineData(".jpg", 3, 1600, 960, 3, 2, 1, 0)]
		[InlineData(".jpg", 8, 960, 1600, 1, 3, 0, 2)]
		[InlineData(".png", 1, 1600, 960, 0, 1, 2, 3)]
		[InlineData(".png", 6, 960, 1600, 2, 0, 3, 1)]
		[InlineData(".png", 3, 1600, 960, 3, 2, 1, 0)]
		[InlineData(".png", 8, 960, 1600, 1, 3, 0, 2)]
		public async Task Issue38570_RotateImageAppliesExifOrientationExactlyOnce(
			string extension, int orientation, int expectedWidth, int expectedHeight,
			int topLeft, int topRight, int bottomLeft, int bottomRight)
		{
			using var image = new InMemoryRandomAccessStream();
			await CreateImageAsync(image, (ushort)orientation);

			// Verify the fixture has unrotated pixels and the requested EXIF tag.
			// Otherwise a faulty fixture could make the regression test pass.
			var sourceDecoder = await BitmapDecoder.CreateAsync(image);
			var properties = await sourceDecoder.BitmapProperties.GetPropertiesAsync(
				new[] { "System.Photo.Orientation" });
			Assert.Equal((ushort)orientation,
				Assert.IsType<ushort>(properties["System.Photo.Orientation"].Value));
			await AssertPixelsAsync(sourceDecoder, (int)Width, (int)Height, 0, 1, 2, 3);

			image.Seek(0);
			using var input = image.AsStreamForRead();
			// The filename selects the output encoder; the input is always EXIF JPEG.
			// Orientation 1 intentionally exercises the unchanged-stream early return.
			using var output = await ImageProcessor.RotateImageAsync(input, "Issue38570" + extension);
			if (orientation == 1)
				Assert.Same(input, output);
			else
				Assert.NotSame(input, output);
			output.Position = 0;

			using var result = new InMemoryRandomAccessStream();
			using (var destination = result.AsStreamForWrite())
			{
				await output.CopyToAsync(destination);
				await destination.FlushAsync();
				result.Seek(0);
				var decoder = await BitmapDecoder.CreateAsync(result);
				Assert.Equal(orientation != 1 && extension == ".png"
					? BitmapDecoder.PngDecoderId : BitmapDecoder.JpegDecoderId,
					decoder.DecoderInformation.CodecId);
				Assert.Equal(decoder.PixelWidth, decoder.OrientedPixelWidth);
				Assert.Equal(decoder.PixelHeight, decoder.OrientedPixelHeight);

				// Inspect stored pixels, not auto-oriented display pixels. The processor
				// must bake in one rotation, including leaving orientation 1 unchanged.
				await AssertPixelsAsync(decoder, expectedWidth, expectedHeight,
					topLeft, topRight, bottomLeft, bottomRight);
				var orientedData = await decoder.GetPixelDataAsync(
					BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, new BitmapTransform(),
					ExifOrientationMode.RespectExifOrientation, ColorManagementMode.DoNotColorManage);
				var orientedPixels = orientedData.DetachPixelData();
				AssertColor(orientedPixels, expectedWidth, expectedWidth / 4, expectedHeight / 4, topLeft);
			}
		}

		static async Task CreateImageAsync(InMemoryRandomAccessStream image, ushort orientation)
		{
			var pixels = new byte[Width * Height * 4];
			for (uint y = 0; y < Height; y++)
			{
				for (uint x = 0; x < Width; x++)
				{
					var color = Colors[(y < Height / 2 ? 0 : 2) + (x < Width / 2 ? 0 : 1)];
					var offset = (y * Width + x) * 4;
					pixels[offset] = color[2];
					pixels[offset + 1] = color[1];
					pixels[offset + 2] = color[0];
					pixels[offset + 3] = 255;
				}
			}

			var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, image);
			encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
				Width, Height, 96, 96, pixels);
			await encoder.BitmapProperties.SetPropertiesAsync(new BitmapPropertySet
			{
				["System.Photo.Orientation"] = new BitmapTypedValue(orientation, PropertyType.UInt16)
			});
			await encoder.FlushAsync();
			image.Seek(0);
		}

		static async Task AssertPixelsAsync(BitmapDecoder decoder, int width, int height,
			int topLeft, int topRight, int bottomLeft, int bottomRight)
		{
			Assert.Equal((uint)width, decoder.PixelWidth);
			Assert.Equal((uint)height, decoder.PixelHeight);
			var data = await decoder.GetPixelDataAsync(
				BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, new BitmapTransform(),
				ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage);
			var pixels = data.DetachPixelData();
			Assert.Equal(width * height * 4, pixels.Length);

			// Sample well inside each quadrant, away from JPEG edges. Tolerance
			// permits lossy encoding but cannot confuse any of the four colors.
			AssertColor(pixels, width, width / 4, height / 4, topLeft);
			AssertColor(pixels, width, 3 * width / 4, height / 4, topRight);
			AssertColor(pixels, width, width / 4, 3 * height / 4, bottomLeft);
			AssertColor(pixels, width, 3 * width / 4, 3 * height / 4, bottomRight);
		}

		static void AssertColor(byte[] pixels, int width, int x, int y, int colorIndex)
		{
			var offset = (y * width + x) * 4;
			var expected = Colors[colorIndex];
			for (var channel = 0; channel < 3; channel++)
			{
				var actual = pixels[offset + 2 - channel];
				Assert.True(Math.Abs(actual - expected[channel]) <= 25,
					$"Pixel ({x}, {y}), RGB channel {channel}: expected {expected[channel]} +/-25, actual {actual}.");
			}
		}
	}
}

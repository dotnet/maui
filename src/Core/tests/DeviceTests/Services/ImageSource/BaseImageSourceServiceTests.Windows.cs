using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Xunit;
using WColor = Windows.UI.Color;

namespace Microsoft.Maui.DeviceTests
{
	public abstract partial class BaseImageSourceServiceTests
	{
		public static string CreateBitmapFile(int width, int height, Color color, string filename = null) =>
			CreateBitmapFile(width, height, color.ToWindowsColor(), filename);

		public static string CreateBitmapFile(int width, int height, WColor color, string filename = null)
		{
			filename ??= Guid.NewGuid().ToString("N") + ".png";
			if (!Path.IsPathRooted(filename))
			{
				filename = Path.Combine(FileSystem.CacheDirectory, Guid.NewGuid().ToString("N"), filename);
			}
			var dir = Path.GetDirectoryName(filename);
			Directory.CreateDirectory(dir);

			using var src = CreateBitmapStream(width, height, color);
			using var dst = File.Create(filename);
			src.CopyTo(dst);

			return filename;
		}

		public static Stream CreateBitmapStream(int width, int height, Color color) =>
			CreateBitmapStream(width, height, color.ToWindowsColor());

		public static Stream CreateBitmapStream(int width, int height, WColor color)
		{
			var bitmap = CreateBitmap(width, height, color);

			var stream = new MemoryStream();

			var encoder = BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream.AsRandomAccessStream()).GetAwaiter().GetResult();

			encoder.SetPixelData(
				BitmapPixelFormat.Bgra8,
				BitmapAlphaMode.Ignore,
				(uint)bitmap.PixelWidth,
				(uint)bitmap.PixelHeight,
				96,
				96,
				bitmap.PixelBuffer.ToArray());

			stream.Position = 0;

			return stream;
		}

		public static WriteableBitmap CreateBitmap(int width, int height, Color color) =>
			CreateBitmap(width, height, color.ToWindowsColor());

		public static WriteableBitmap CreateBitmap(int width, int height, WColor color)
		{
			var bitmap = new WriteableBitmap(width, height);

			using (var stream = bitmap.PixelBuffer.AsStream())
			{
				var pixels = new byte[width * height * 4];

				for (var y = 0; y < height; y++)
				{
					for (var x = 0; x < width; x++)
					{
						var index = (y * width + x) * 4;

						pixels[index + 0] = color.B;
						pixels[index + 1] = color.G;
						pixels[index + 2] = color.R;
						pixels[index + 3] = color.A;
					}
				}

				stream.Write(pixels, 0, pixels.Length);
			}

			bitmap.Invalidate();

			return bitmap;
		}
	}
}
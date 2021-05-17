using System;
using System.IO;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Graphics;
using Xunit;
using ABitmap = Android.Graphics.Bitmap;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public abstract partial class BaseImageSourceServiceTests
	{
		protected string CreateBitmapFile(int width, int height, Color color, string filename = null) =>
			CreateBitmapFile(width, height, color.ToNative(), filename);

		protected string CreateBitmapFile(int width, int height, AColor color, string filename = null)
		{
			filename ??= Guid.NewGuid().ToString("N") + ".png";
			if (!Path.IsPathRooted(filename))
				filename = Path.Combine(FileSystem.CacheDirectory, Guid.NewGuid().ToString("N"), filename);
			var dir = Path.GetDirectoryName(filename);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			using var src = CreateBitmapStream(width, height, color);
			using var dst = File.Create(filename);
			src.CopyTo(dst);

			return filename;
		}

		protected Stream CreateBitmapStream(int width, int height, Color color) =>
			CreateBitmapStream(width, height, color.ToNative());

		protected Stream CreateBitmapStream(int width, int height, AColor color)
		{
			using var bitmap = CreateBitmap(width, height, color);

			var stream = new MemoryStream();

			var success = bitmap.Compress(ABitmap.CompressFormat.Png, 100, stream);
			Assert.True(success);

			stream.Position = 0;

			return stream;
		}

		protected ABitmap CreateBitmap(int width, int height, Color color) =>
			CreateBitmap(width, height, color.ToNative());

		protected ABitmap CreateBitmap(int width, int height, AColor color)
		{
			var bitmap = ABitmap.CreateBitmap(width, height, ABitmap.Config.Argb8888);

			bitmap.EraseColor(color);

			return bitmap;
		}
	}
}
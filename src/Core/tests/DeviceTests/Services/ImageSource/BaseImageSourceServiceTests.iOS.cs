using System;
using System.IO;
using CoreGraphics;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Graphics;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public abstract partial class BaseImageSourceServiceTests
	{
		protected string CreateBitmapFile(int width, int height, Color color, string filename = null) =>
			CreateBitmapFile(width, height, color.ToNative(), filename);

		protected string CreateBitmapFile(int width, int height, UIColor color, string filename = null)
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

		protected Stream CreateBitmapStream(int width, int height, UIColor color)
		{
			using var bitmap = CreateBitmap(width, height, color);

			var stream = new MemoryStream();

			using var png = bitmap.AsPNG();
			using var srcStream = png.AsStream();
			srcStream.CopyTo(stream);

			stream.Position = 0;

			return stream;
		}

		protected UIImage CreateBitmap(int width, int height, Color color) =>
			CreateBitmap(width, height, color.ToNative());

		protected UIImage CreateBitmap(int width, int height, UIColor color)
		{
			var rect = new CGRect(0, 0, width, height);

			UIGraphics.BeginImageContextWithOptions(rect.Size, false, 1);
			var context = UIGraphics.GetCurrentContext();

			color.SetFill();
			context.FillRect(rect);

			var image = UIGraphics.GetImageFromCurrentImageContext();

			UIGraphics.EndImageContext();

			return image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
		}
	}
}
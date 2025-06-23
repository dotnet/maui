using System;
using System.IO;
using CoreGraphics;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public abstract partial class BaseImageSourceServiceTests
	{
		public static string CreateBitmapFile(int width, int height, Color color, string filename = null) =>
			CreateBitmapFile(width, height, color.ToPlatform(), filename);

		public static string CreateBitmapFile(int width, int height, UIColor color, string filename = null)
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

		public static Stream CreateBitmapStream(int width, int height, Color color) =>
			CreateBitmapStream(width, height, color.ToPlatform());

		public static Stream CreateBitmapStream(int width, int height, UIColor color)
		{
			using var bitmap = CreateBitmap(width, height, color);

			var stream = new MemoryStream();

			using var png = bitmap.AsPNG();
			using var srcStream = png.AsStream();
			srcStream.CopyTo(stream);

			stream.Position = 0;

			return stream;
		}

		public static UIImage CreateBitmap(int width, int height, Color color) =>
			CreateBitmap(width, height, color.ToPlatform());

		public static UIImage CreateBitmap(int width, int height, UIColor color)
		{
			var rect = new CGRect(0, 0, width, height);

			var renderer = new UIGraphicsImageRenderer(rect, new UIGraphicsImageRendererFormat()
			{
				Opaque = false,
				Scale = 1,
			});

			return renderer.CreateImage((context) =>
			{
				color.SetFill();
				context.FillRect(rect);
			}).ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
		}
	}
}
using System;
using System.IO;
using SharpDX;
using SharpDX.DXGI;
using d2 = SharpDX.Direct2D1;
using wic = SharpDX.WIC;

namespace Microsoft.Maui.Graphics.SharpDX
{
	public static class D2BitmapExtensions
	{
		public static byte[] AsPNG(this d2.Bitmap target)
		{
			byte[] bytes;

			using (var stream = new MemoryStream())
			{
				EncodeImage(target, ImageFormat.Png, stream);
				bytes = stream.ToArray();
			}

			return bytes;
		}

		public static byte[] AsJPEG(this d2.Bitmap target)
		{
			byte[] bytes;

			using (var stream = new MemoryStream())
			{
				EncodeImage(target, ImageFormat.Jpeg, stream);
				bytes = stream.ToArray();
			}

			return bytes;
		}

		public static byte[] AsBMP(this d2.Bitmap target)
		{
			byte[] bytes;

			using (var stream = new MemoryStream())
			{
				EncodeImage(target, ImageFormat.Bmp, stream);
				bytes = stream.ToArray();
			}

			return bytes;
		}

		public static void EncodeImage(this d2.Bitmap target, ImageFormat imageFormat, Stream outputStream)
		{
			var width = target.PixelSize.Width;
			var height = target.PixelSize.Height;

			var wicBitmap = new wic.Bitmap(DXGraphicsService.FactoryImaging, width, height,
				wic.PixelFormat.Format32bppBGR, wic.BitmapCreateCacheOption.CacheOnLoad);
			var renderTargetProperties = new d2.RenderTargetProperties(d2.RenderTargetType.Default,
				new d2.PixelFormat(Format.Unknown,
					d2.AlphaMode.Unknown), 0, 0,
				d2.RenderTargetUsage.None,
				d2.FeatureLevel.Level_DEFAULT);
			var d2DRenderTarget = new d2.WicRenderTarget(target.Factory, wicBitmap, renderTargetProperties);

			d2DRenderTarget.BeginDraw();
			d2DRenderTarget.Clear(global::SharpDX.Color.Transparent);
			d2DRenderTarget.DrawBitmap(target, 1, d2.BitmapInterpolationMode.Linear);
			d2DRenderTarget.EndDraw();

			var stream = new wic.WICStream(DXGraphicsService.FactoryImaging, outputStream);

			// Initialize a Jpeg encoder with this stream
			var encoder = new wic.BitmapEncoder(DXGraphicsService.FactoryImaging, GetImageFormat(imageFormat));
			encoder.Initialize(stream);

			// Create a Frame encoder
			var bitmapFrameEncode = new wic.BitmapFrameEncode(encoder);
			bitmapFrameEncode.Initialize();
			bitmapFrameEncode.SetSize(width, height);
			Guid pixelFormatGuid = wic.PixelFormat.FormatDontCare;
			bitmapFrameEncode.SetPixelFormat(ref pixelFormatGuid);
			bitmapFrameEncode.WriteSource(wicBitmap);

			bitmapFrameEncode.Commit();
			encoder.Commit();

			bitmapFrameEncode.Dispose();
			encoder.Dispose();
			stream.Dispose();

			d2DRenderTarget.Dispose();
			wicBitmap.Dispose();
		}

		private static Guid GetImageFormat(ImageFormat format)
		{
			switch (format)
			{
				case ImageFormat.Bmp:
					return wic.ContainerFormatGuids.Bmp;
				case ImageFormat.Ico:
					return wic.ContainerFormatGuids.Ico;
				case ImageFormat.Gif:
					return wic.ContainerFormatGuids.Gif;
				case ImageFormat.Jpeg:
					return wic.ContainerFormatGuids.Jpeg;
				case ImageFormat.Png:
					return wic.ContainerFormatGuids.Png;
				case ImageFormat.Tiff:
					return wic.ContainerFormatGuids.Tiff;
				case ImageFormat.Wmp:
					return wic.ContainerFormatGuids.Wmp;
			}

			throw new NotSupportedException();
		}
	}

	public enum ImageFormat
	{
		Png,
		Gif,
		Ico,
		Jpeg,
		Wmp,
		Tiff,
		Bmp
	}
}

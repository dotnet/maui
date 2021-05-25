using System;
using System.Diagnostics;
using SkiaSharp;

namespace Microsoft.Maui.Resizetizer
{
	internal class SkiaSharpBitmapTools : SkiaSharpTools, IDisposable
	{
		SKBitmap bmp;

		public SkiaSharpBitmapTools(ResizeImageInfo info, ILogger logger)
			: this(info.Filename, info.BaseSize, info.TintColor, logger)
		{
		}

		public SkiaSharpBitmapTools(string filename, SKSize? baseSize, SKColor? tintColor, ILogger logger)
			: base(filename, baseSize, tintColor, logger)
		{
			var sw = new Stopwatch();
			sw.Start();

			bmp = SKBitmap.Decode(filename);

			sw.Stop();
			Logger?.Log($"Open RASTER took {sw.ElapsedMilliseconds}ms ({filename})");
		}

		public override SKSize GetOriginalSize() =>
			bmp.Info.Size;

		public override void DrawUnscaled(SKCanvas canvas, float scale) =>
			canvas.DrawBitmap(bmp, 0, 0, Paint);

		public void Dispose()
		{
			bmp?.Dispose();
			bmp = null;
		}
	}
}

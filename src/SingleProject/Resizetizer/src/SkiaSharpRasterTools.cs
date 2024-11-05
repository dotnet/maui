using System;
using System.Diagnostics;
using SkiaSharp;

namespace Microsoft.Maui.Resizetizer
{
	internal class SkiaSharpRasterTools : SkiaSharpTools, IDisposable
	{
		SKImage img;

		public SkiaSharpRasterTools(ResizeImageInfo info, ILogger logger)
			: this(info.Filename, info.BaseSize, info.Color, info.TintColor, info.FilterQuality, logger)
		{
		}

#pragma warning disable CS0618 // Type or member is obsolete
		public SkiaSharpRasterTools(string filename, SKSize? baseSize, SKColor? backgroundColor, SKColor? tintColor, SKFilterQuality? filterQuality, ILogger logger)
#pragma warning restore CS0618 // Type or member is obsolete
			: base(filename, baseSize, backgroundColor, tintColor, filterQuality, logger)
		{
			var sw = new Stopwatch();
			sw.Start();

			img = SKImage.FromEncodedData(filename);

			sw.Stop();
			Logger?.Log($"Open RASTER took {sw.ElapsedMilliseconds}ms ({filename})");
		}

		public override SKSize GetOriginalSize() =>
			img.Info.Size;

		public override void DrawUnscaled(SKCanvas canvas, float scale) =>
			canvas.DrawImage(img, 0, 0, SamplingOptions, Paint);

		public void Dispose()
		{
			img?.Dispose();
			img = null;
		}
	}
}

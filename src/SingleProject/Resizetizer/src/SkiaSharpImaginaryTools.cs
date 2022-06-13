using System;
using SkiaSharp;

namespace Microsoft.Maui.Resizetizer
{
	internal class SkiaSharpImaginaryTools : SkiaSharpTools, IDisposable
	{
		public SkiaSharpImaginaryTools(ResizeImageInfo info, ILogger logger)
			: this(info.BaseSize, info.Color, logger)
		{
		}

		public SkiaSharpImaginaryTools(SKSize? baseSize, SKColor? backgroundColor, ILogger logger)
			: base(null, baseSize, backgroundColor, null, logger)
		{
		}

		public override SKSize GetOriginalSize() =>
			new SKSize(1, 1);

		public override void DrawUnscaled(SKCanvas canvas, float scale)
		{
			// no-op
		}

		public void Dispose()
		{
		}
	}
}

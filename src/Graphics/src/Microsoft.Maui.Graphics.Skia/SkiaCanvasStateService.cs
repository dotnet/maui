using SkiaSharp;
using System;

namespace Microsoft.Maui.Graphics.Skia
{
	public class SkiaCanvasStateService : IDisposable, ICanvasStateService<SkiaCanvasState>
	{
		private static SKPaint _defaultFillPaint;
		private static SKPaint _defaultFontPaint;
		private static SKPaint _defaultStrokePaint;

		public SkiaCanvasState CreateNew(object context)
		{
			EnsureDefaults();

			var state = new SkiaCanvasState
			{
				FillPaint = _defaultFillPaint.CreateCopy(),
				StrokePaint = _defaultStrokePaint.CreateCopy(),
				FontPaint = _defaultFontPaint.CreateCopy(),
			};

			return state;
		}

		public SkiaCanvasState CreateCopy(SkiaCanvasState prototype) =>
			new SkiaCanvasState(prototype);

		public void Reset(SkiaCanvasState currentState)
		{
			currentState.Reset(_defaultFontPaint, _defaultFillPaint, _defaultStrokePaint);
		}

		private void EnsureDefaults()
		{
			if (_defaultFillPaint != null)
				return;

			_defaultFillPaint = new SKPaint
			{
				Color = SKColors.White,
				IsStroke = false,
				IsAntialias = true
			};

			_defaultStrokePaint = new SKPaint
			{
				Color = SKColors.Black,
				StrokeWidth = 1,
				StrokeMiter = CanvasDefaults.DefaultMiterLimit,
				IsStroke = true,
				IsAntialias = true
			};

			_defaultFontPaint = new SKPaint
			{
				Color = SKColors.Black,
				IsAntialias = true,
				Typeface = SKTypeface.FromFamilyName("Arial")
			};
		}

		public void Dispose()
		{
			_defaultFillPaint.Dispose();
			_defaultStrokePaint.Dispose();
			_defaultFontPaint.Dispose();
		}
	}
}

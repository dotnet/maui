using System;
using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
	/// <summary>
	/// Provides services for creating and managing <see cref="SkiaCanvasState"/> instances.
	/// </summary>
	public class SkiaCanvasStateService : IDisposable, ICanvasStateService<SkiaCanvasState>
	{
		private SKPaint _defaultFillPaint;
		private SKPaint _defaultFontPaint;
		private SKFont _defaultFontFont;
		private SKPaint _defaultStrokePaint;

		/// <summary>
		/// Creates a new canvas state instance.
		/// </summary>
		/// <param name="context">An optional context object.</param>
		/// <returns>A new <see cref="SkiaCanvasState"/> instance with default properties.</returns>
		public SkiaCanvasState CreateNew(object context)
		{
			EnsureDefaults();

			var state = new SkiaCanvasState
			{
				FillPaint = _defaultFillPaint.CreateCopy(),
				StrokePaint = _defaultStrokePaint.CreateCopy(),
				FontPaint = _defaultFontPaint.CreateCopy(),
				FontFont = _defaultFontFont.CreateCopy(),
			};

			return state;
		}

		/// <summary>
		/// Creates a copy of the specified canvas state.
		/// </summary>
		/// <param name="prototype">The canvas state to copy.</param>
		/// <returns>A new <see cref="SkiaCanvasState"/> instance with properties copied from the prototype.</returns>
		public SkiaCanvasState CreateCopy(SkiaCanvasState prototype) =>
			new SkiaCanvasState(prototype);

		/// <summary>
		/// Resets the specified canvas state to default values.
		/// </summary>
		/// <param name="currentState">The canvas state to reset.</param>
		public void Reset(SkiaCanvasState currentState)
		{
			currentState.Reset(_defaultFontPaint, _defaultFontFont, _defaultFillPaint, _defaultStrokePaint);
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
#pragma warning disable CS0618 // Type or member is obsolete
				Typeface = SKTypeface.FromFamilyName("Arial")
#pragma warning restore CS0618 // Type or member is obsolete
			};

			_defaultFontFont = new SKFont
			{
				Typeface = SKTypeface.FromFamilyName("Arial")
			};
		}

		/// <summary>
		/// Releases all resources used by this canvas state service.
		/// </summary>
		public void Dispose()
		{
			_defaultFillPaint.Dispose();
			_defaultStrokePaint.Dispose();
			_defaultFontPaint.Dispose();
			_defaultFontFont.Dispose();
		}
	}
}

using System;
using Android.Graphics;
using Android.Text;

namespace Microsoft.Maui.Graphics.Platform
{
	public class PlatformCanvasStateService : IDisposable, ICanvasStateService<PlatformCanvasState>
	{
		private global::Android.Graphics.Paint _defaultFillPaint;
		private TextPaint _defaultFontPaint;
		private global::Android.Graphics.Paint _defaultStrokePaint;

		public PlatformCanvasState CreateNew(object context)
		{
			EnsureDefaults();

			var state = new PlatformCanvasState
			{
				FillPaint = new global::Android.Graphics.Paint(_defaultFillPaint),
				StrokePaint = new global::Android.Graphics.Paint(_defaultStrokePaint),
				FontPaint = new TextPaint(_defaultFontPaint),
				Font = null
			};

			return state;
		}

		public PlatformCanvasState CreateCopy(PlatformCanvasState prototype) =>
			new PlatformCanvasState(prototype);

		public void Reset(PlatformCanvasState currentState)
		{
			currentState.Reset(_defaultFontPaint, _defaultFillPaint, _defaultStrokePaint);
		}

		private void EnsureDefaults()
		{
			if (_defaultFillPaint != null)
				return;

			_defaultFillPaint = new global::Android.Graphics.Paint();
			_defaultFillPaint.SetARGB(255, 255, 255, 255);
			_defaultFillPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
			_defaultFillPaint.AntiAlias = true;

			_defaultStrokePaint = new global::Android.Graphics.Paint();
			_defaultStrokePaint.SetARGB(255, 0, 0, 0);
			_defaultStrokePaint.StrokeWidth = 1;
			_defaultStrokePaint.StrokeMiter = CanvasDefaults.DefaultMiterLimit;
			_defaultStrokePaint.SetStyle(global::Android.Graphics.Paint.Style.Stroke);
			_defaultStrokePaint.AntiAlias = true;

			_defaultFontPaint = new TextPaint();
			_defaultFontPaint.SetARGB(255, 0, 0, 0);
			_defaultFontPaint.AntiAlias = true;

			var defaultFont = Typeface.Default;
			if (defaultFont != null)
				_defaultFontPaint.SetTypeface(defaultFont);
			else
				System.Diagnostics.Debug.WriteLine("Unable to set the default font paint to Default");
		}

		public void Dispose()
		{
			_defaultFillPaint.Dispose();
			_defaultStrokePaint.Dispose();
			_defaultFontPaint.Dispose();
		}
	}
}

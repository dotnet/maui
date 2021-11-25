using System;

namespace Microsoft.Maui.Graphics.Platform.Gtk {

	public class PlatformCanvasState : CanvasState {

		public PlatformCanvasState() {
			Alpha = 1;

			StrokeColor = Colors.Black.ToCairoColor();
			FontColor = StrokeColor;
			FillColor = Colors.White.ToCairoColor();

			MiterLimit = 10;
			LineJoin = Cairo.LineJoin.Miter;
			LineCap = Cairo.LineCap.Butt;
		}

		public PlatformCanvasState(PlatformCanvasState prototype)
			: base(prototype)
		{
			Antialias = prototype.Antialias;
			MiterLimit = prototype.MiterLimit;
			StrokeColor = prototype.StrokeColor;
			LineCap = prototype.LineCap;
			LineJoin = prototype.LineJoin;
			FillColor = prototype.FillColor;

			Font = prototype.Font;
			FontSize = prototype.FontSize;
			FontColor = prototype.FontColor;

			BlendMode = prototype.BlendMode;
			Alpha = prototype.Alpha;

			Shadow = prototype.Shadow;
			FillPaint = prototype.FillPaint;

		}

		public Cairo.Antialias Antialias { get; set; }

		public double MiterLimit { get; set; }

		public Cairo.Color StrokeColor { get; set; }

		public Cairo.LineCap LineCap { get; set; }

		public Cairo.LineJoin LineJoin { get; set; }

		public Cairo.Color FillColor { get; set; }

		public Cairo.Color FontColor { get; set; }

		public IFont Font { get; set; }

		public float FontSize { get; set; }

		public BlendMode BlendMode { get; set; }

		public float Alpha { get; set; }

		private readonly double[] zerodash = new double[0];

		public double[] NativeDash => StrokeDashPattern != null ? Array.ConvertAll(StrokeDashPattern, f => (double) f) : zerodash;

		public (SizeF offset, float blur, Color color) Shadow { get; set; }

		public (Paint paint, RectangleF rectangle) FillPaint { get; set; }

		public override void Dispose() {

			FillPaint = default;
			Shadow = default;
			StrokeDashPattern = default;

			base.Dispose();
		}

	}

}

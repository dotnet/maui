using System;

namespace Microsoft.Maui.Graphics.Cairo
{
	/// <summary>
	/// The drawing state of a <see cref="CairoCanvas"/>: everything that <see cref="ICanvas"/> lets callers
	/// set and that Cairo itself does not keep (colors, fonts, stroke style, shadow, blur, fill paint).
	/// </summary>
	/// <remarks>
	/// Transform and clip live in the Cairo context (<c>cairo_save</c>/<c>cairo_restore</c>); this class holds the rest.
	/// </remarks>
	public class CairoCanvasState : CanvasState
	{
		private CairoPattern _fillPattern;
		private bool _ownsFillPattern;

		/// <summary>Gets or sets the stroke color.</summary>
		public Color StrokeColor { get; set; } = Colors.Black;

		/// <summary>Gets or sets the fill color (used when <see cref="FillPattern"/> is <see langword="null"/>).</summary>
		public Color FillColor { get; set; } = Colors.White;

		/// <summary>Gets or sets the text color.</summary>
		public Color FontColor { get; set; } = Colors.Black;

		/// <summary>Gets or sets the font used by the string drawing methods.</summary>
		public IFont Font { get; set; }

		/// <summary>Gets or sets the font size, in user units.</summary>
		public float FontSize { get; set; } = 12f;

		/// <summary>Gets or sets the global alpha (0..1) applied to everything that is drawn.</summary>
		public float Alpha { get; set; } = 1f;

		/// <summary>Gets or sets whether anti-aliasing is enabled.</summary>
		public bool Antialias { get; set; } = true;

		/// <summary>Gets or sets the blend mode.</summary>
		public BlendMode BlendMode { get; set; } = BlendMode.Normal;

		/// <summary>Gets or sets the stroke line cap.</summary>
		public LineCap StrokeLineCap { get; set; } = LineCap.Butt;

		/// <summary>Gets or sets the stroke line join.</summary>
		public LineJoin StrokeLineJoin { get; set; } = LineJoin.Miter;

		/// <summary>Gets or sets the miter limit.</summary>
		public float MiterLimit { get; set; } = CanvasDefaults.DefaultMiterLimit;

		/// <summary>Gets or sets the stroke width actually handed to Cairo, in user units.</summary>
		public float PlatformStrokeSize { get; set; } = 1f;

		/// <summary>Gets or sets the dash lengths in user units (already multiplied by the stroke size), or <see langword="null"/> for a solid line.</summary>
		public double[] PlatformDashPattern { get; set; }

		/// <summary>Gets or sets the dash offset in user units.</summary>
		public double PlatformDashOffset { get; set; }

		/// <summary>Gets whether a drop shadow is active.</summary>
		public bool HasShadow { get; private set; }

		/// <summary>Gets the shadow offset, in device pixels.</summary>
		public SizeF ShadowOffset { get; private set; }

		/// <summary>Gets the shadow blur radius, in device pixels.</summary>
		public float ShadowBlur { get; private set; }

		/// <summary>Gets the shadow color (alpha included, before the global <see cref="Alpha"/>).</summary>
		public Color ShadowColor { get; private set; }

		/// <summary>Gets or sets the blur radius applied to everything that is drawn (<see cref="IBlurrableCanvas"/>), in device pixels.</summary>
		public float BlurRadius { get; set; }

		/// <summary>
		/// Gets the Cairo pattern (gradient or image) used for fills, or <see langword="null"/> when <see cref="FillColor"/> applies.
		/// </summary>
		public CairoPattern FillPattern => _fillPattern;

		/// <summary>Creates a state with the default values.</summary>
		public CairoCanvasState()
		{
		}

		/// <summary>Creates a copy of <paramref name="prototype"/>.</summary>
		/// <param name="prototype">The state to copy.</param>
		public CairoCanvasState(CairoCanvasState prototype) : base(prototype)
		{
			StrokeColor = prototype.StrokeColor;
			FillColor = prototype.FillColor;
			FontColor = prototype.FontColor;
			Font = prototype.Font;
			FontSize = prototype.FontSize;
			Alpha = prototype.Alpha;
			Antialias = prototype.Antialias;
			BlendMode = prototype.BlendMode;
			StrokeLineCap = prototype.StrokeLineCap;
			StrokeLineJoin = prototype.StrokeLineJoin;
			MiterLimit = prototype.MiterLimit;
			PlatformStrokeSize = prototype.PlatformStrokeSize;
			PlatformDashPattern = prototype.PlatformDashPattern;
			PlatformDashOffset = prototype.PlatformDashOffset;
			HasShadow = prototype.HasShadow;
			ShadowOffset = prototype.ShadowOffset;
			ShadowBlur = prototype.ShadowBlur;
			ShadowColor = prototype.ShadowColor;
			BlurRadius = prototype.BlurRadius;

			// The copy only borrows the pattern: the state that created it disposes it, and it always outlives its copies
			// (states are strictly nested on the canvas state stack).
			_fillPattern = prototype._fillPattern;
			_ownsFillPattern = false;
		}

		/// <summary>Sets the drop shadow.</summary>
		/// <param name="offset">The shadow offset in device pixels.</param>
		/// <param name="blur">The blur radius in device pixels (0 for a hard shadow).</param>
		/// <param name="color">The shadow color.</param>
		public void SetShadow(SizeF offset, float blur, Color color)
		{
			HasShadow = true;
			ShadowOffset = offset;
			ShadowBlur = Math.Max(0, blur);
			ShadowColor = color ?? Colors.Black;
		}

		/// <summary>Removes the drop shadow.</summary>
		public void ClearShadow()
		{
			HasShadow = false;
			ShadowOffset = default;
			ShadowBlur = 0;
			ShadowColor = null;
		}

		/// <summary>Sets the pattern used for fills, taking ownership of it. Pass <see langword="null"/> to go back to <see cref="FillColor"/>.</summary>
		/// <param name="pattern">The pattern, or <see langword="null"/>.</param>
		public void SetFillPattern(CairoPattern pattern)
		{
			ReleaseFillPattern();
			_fillPattern = pattern;
			_ownsFillPattern = pattern != null;
		}

		/// <summary>Resets every property to its default value.</summary>
		public void Reset()
		{
			ReleaseFillPattern();
			_fillPattern = null;
			_ownsFillPattern = false;

			StrokeColor = Colors.Black;
			FillColor = Colors.White;
			FontColor = Colors.Black;
			Font = null;
			FontSize = 12f;
			Alpha = 1f;
			Antialias = true;
			BlendMode = BlendMode.Normal;
			StrokeLineCap = LineCap.Butt;
			StrokeLineJoin = LineJoin.Miter;
			MiterLimit = CanvasDefaults.DefaultMiterLimit;
			PlatformStrokeSize = 1f;
			PlatformDashPattern = null;
			PlatformDashOffset = 0;
			HasShadow = false;
			ShadowOffset = default;
			ShadowBlur = 0;
			ShadowColor = null;
			BlurRadius = 0;
			StrokeSize = 1;
			StrokeDashPattern = null;
			StrokeDashOffset = 1;
			Transform = System.Numerics.Matrix3x2.Identity;
		}

		private void ReleaseFillPattern()
		{
			if (_ownsFillPattern)
				_fillPattern?.Dispose();
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
			ReleaseFillPattern();
			_fillPattern = null;
			_ownsFillPattern = false;
			base.Dispose();
		}
	}
}

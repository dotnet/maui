using System;
using SkiaSharp;
using Color = SkiaSharp.SKColor;

namespace Microsoft.Maui.Graphics.Skia
{
	/// <summary>
	/// Represents the state of a <see cref="SkiaCanvas"/>, maintaining properties like colors, fonts, and transformations.
	/// </summary>
	public class SkiaCanvasState : CanvasState, IBlurrableCanvas
	{
		public float Alpha = 1;
		private SKPaint _fillPaint;
		private SKPaint _strokePaint;
		private IFont _font;
		private SKPaint _fontPaint;
		private SKFont _fontFont;
		private float _fontSize = 10f;
		private float _scaleX = 1;
		private float _scaleY = 1;
		private bool _isBlurred;
		private float _blurRadius;
		private SKMaskFilter _blurFilter;
		private SKImageFilter _shadowFilter;
		private bool _shadowed;
		private SKColor _shadowColor;
		private float _shadowX;
		private float _shadowY;
		private float _shadowBlur;

		private Color _strokeColor = Colors.Black;
		private Color _fillColor = Colors.White;
		private Color _fontColor = Colors.Black;

		/// <summary>
		/// Initializes a new instance of the <see cref="SkiaCanvasState"/> class.
		/// </summary>
		public SkiaCanvasState()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SkiaCanvasState"/> class with properties copied from another state.
		/// </summary>
		/// <param name="prototype">The state to copy from.</param>
		public SkiaCanvasState(SkiaCanvasState prototype) : base(prototype)
		{
			_strokeColor = prototype._strokeColor;
			_fillColor = prototype._fillColor;
			_fontColor = prototype._fontColor;

			_fontPaint = prototype.FontPaint.CreateCopy();
			_fontFont = prototype.FontFont.CreateCopy();
			_fillPaint = prototype.FillPaint.CreateCopy();
			_strokePaint = prototype.StrokePaint.CreateCopy();
			_font = prototype._font;
			_fontSize = prototype._fontSize;
			Alpha = prototype.Alpha;
			_scaleX = prototype._scaleX;
			_scaleY = prototype._scaleY;

			_isBlurred = prototype._isBlurred;
			_blurRadius = prototype._blurRadius;

			_shadowed = prototype._shadowed;
			//_shadowFilter = prototype._shadowFilter;  // There is no reason the copy really needs to know about this.
			_shadowColor = prototype._shadowColor;
			_shadowX = prototype._shadowX;
			_shadowY = prototype._shadowY;
			_shadowBlur = prototype._shadowBlur;
		}

		/// <summary>
		/// Gets or sets the stroke color.
		/// </summary>
		public Color StrokeColor
		{
			get => _strokeColor;
			set => _strokeColor = value;
		}

		/// <summary>
		/// Gets or sets the fill color.
		/// </summary>
		/// <remarks>
		/// Setting this property resets any shader that might have been set.
		/// </remarks>
		public Color FillColor
		{
			get => _fillColor;
			set
			{
				FillPaint.Shader = null;
				_fillColor = value;
			}
		}

		/// <summary>
		/// Gets or sets the font color.
		/// </summary>
		public Color FontColor
		{
			get => _fontColor;
			set
			{
				_fontColor = value;
				if (value != null)
					FontPaint.Color = _fontColor.AsSKColor();
				else
					FontPaint.Color = SKColors.Black;
			}
		}

		/// <summary>
		/// Sets the stroke line cap style.
		/// </summary>
		/// <value>The line cap style to apply to the ends of stroked lines.</value>
		public LineCap StrokeLineCap
		{
			set
			{
				if (value == LineCap.Butt)
					StrokePaint.StrokeCap = SKStrokeCap.Butt;
				else if (value == LineCap.Round)
					StrokePaint.StrokeCap = SKStrokeCap.Round;
				else if (value == LineCap.Square)
					StrokePaint.StrokeCap = SKStrokeCap.Square;
			}
		}

		/// <summary>
		/// Sets the stroke line join style.
		/// </summary>
		/// <value>The line join style to apply at the corners of stroked lines.</value>
		public LineJoin StrokeLineJoin
		{
			set
			{
				if (value == LineJoin.Miter)
					StrokePaint.StrokeJoin = SKStrokeJoin.Miter;
				else if (value == LineJoin.Round)
					StrokePaint.StrokeJoin = SKStrokeJoin.Round;
				else if (value == LineJoin.Bevel)
					StrokePaint.StrokeJoin = SKStrokeJoin.Bevel;
			}
		}

		/// <summary>
		/// Sets the miter limit for stroked lines with miter joins.
		/// </summary>
		/// <value>The miter limit value.</value>
		public float MiterLimit
		{
			set => StrokePaint.StrokeMiter = value;
		}

		/// <summary>
		/// Sets the stroke dash pattern.
		/// </summary>
		/// <param name="pattern">An array of values that specify the lengths of alternating dashes and gaps.</param>
		/// <param name="strokeDashOffset">The distance into the dash pattern to start the dash.</param>
		/// <param name="strokeSize">The stroke width to scale the pattern by.</param>
		public void SetStrokeDashPattern(float[] pattern, float strokeDashOffset, float strokeSize)
		{
			if (pattern == null || pattern.Length == 0 || strokeSize == 0)
			{
				StrokePaint.PathEffect = null;
			}
			else
			{
				float scaledStrokeSize = strokeSize * ScaleX;

				if (scaledStrokeSize > 1 || scaledStrokeSize < 1)
				{
					var scaledPattern = new float[pattern.Length];
					for (var i = 0; i < pattern.Length; i++)
						scaledPattern[i] = pattern[i] * scaledStrokeSize;
					StrokePaint.PathEffect = SKPathEffect.CreateDash(scaledPattern, 0);
				}
				else
				{
					StrokePaint.PathEffect = SKPathEffect.CreateDash(pattern, 0);
				}
			}
		}

		public bool AntiAlias
		{
			set => StrokePaint.IsAntialias = value;
		}

		public bool IsBlurred => _isBlurred;

		public float BlurRadius => _blurRadius;

		public void SetBlur(float radius)
		{
			if (radius != _blurRadius)
			{
				if (_blurFilter != null)
				{
					_blurFilter.Dispose();
					_blurFilter = null;
				}

				if (radius > 0)
				{
					_isBlurred = true;
					_blurRadius = radius;
					_blurFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, _blurRadius);

					if (_fillPaint != null)
						_fillPaint.MaskFilter = _blurFilter;
					if (_strokePaint != null)
						_strokePaint.MaskFilter = _blurFilter;
					if (_fontPaint != null)
						_fontPaint.MaskFilter = _blurFilter;
				}
				else
				{
					_isBlurred = false;
					_blurRadius = 0;

					if (_fillPaint != null)
						_fillPaint.MaskFilter = null;
					if (_strokePaint != null)
						_strokePaint.MaskFilter = null;
					if (_fontPaint != null)
						_fontPaint.MaskFilter = null;
				}
			}
		}

		public float PlatformStrokeSize
		{
			set => StrokePaint.StrokeWidth = value * _scaleX;
		}

		public float FontSize
		{
			set
			{
				_fontSize = value;
#pragma warning disable CS0618 // Type or member is obsolete
				FontPaint.TextSize = _fontSize * _scaleX;
#pragma warning restore CS0618 // Type or member is obsolete
				FontFont.Size = _fontSize * _scaleX;
			}
		}

		public IFont Font
		{
			set
			{
				if (!ReferenceEquals(_font, value) && (_font is null || !_font.Equals(value)))
				{
					_font = value;

					if (_fontPaint != null)
					{
#pragma warning disable CS0618 // Type or member is obsolete
						_fontPaint.Typeface = GetSKTypeface();
#pragma warning restore CS0618 // Type or member is obsolete
					}

					if (_fontFont != null)
					{
						_fontFont.Typeface = GetSKTypeface();
					}
				}
			}

			get => _font;
		}
		private SKTypeface GetSKTypeface() => _font?.ToSKTypeface() ?? SKTypeface.Default;

		public SKPaint FontPaint
		{
			get
			{
				if (_fontPaint == null)
				{
					_fontPaint = new SKPaint
					{
						Color = SKColors.Black,
						IsAntialias = true,
#pragma warning disable CS0618 // Type or member is obsolete
						Typeface = GetSKTypeface(),
#pragma warning restore CS0618 // Type or member is obsolete
					};
				}

				return _fontPaint;
			}

			set => _fontPaint = value;
		}


		public SKFont FontFont
		{
			get
			{
				if (_fontFont == null)
				{
					_fontFont = new SKFont
					{
						Typeface = GetSKTypeface(),
					};
				}

				return _fontFont;
			}

			set => _fontFont = value;
		}

		public SKPaint FillPaint
		{
			private get
			{
				return _fillPaint
					   ?? (_fillPaint = new SKPaint
					   {
						   Color = SKColors.White,
						   IsStroke = false,
						   IsAntialias = true
					   });
			}

			set { _fillPaint = value; }
		}

		public SKPaint StrokePaint
		{
			private get
			{
				if (_strokePaint == null)
				{
					var paint = new SKPaint
					{
						Color = SKColors.Black,
						StrokeWidth = 1,
						StrokeMiter = CanvasDefaults.DefaultMiterLimit,
						IsStroke = true,
						IsAntialias = true
					};

					_strokePaint = paint;

					return paint;
				}

				return _strokePaint;
			}

			set { _strokePaint = value; }
		}

		public SKPaint StrokePaintWithAlpha
		{
			get
			{
				var paint = StrokePaint;

				var r = (byte)(_strokeColor.Red * 255f);
				var g = (byte)(_strokeColor.Green * 255f);
				var b = (byte)(_strokeColor.Blue * 255f);
				var a = (byte)(_strokeColor.Alpha * 255f * Alpha);

				paint.Color = new SKColor(r, g, b, a);
				return paint;
			}
		}

		public SKPaint FillPaintWithAlpha
		{
			get
			{
				var paint = FillPaint;

				var r = (byte)(_fillColor.Red * 255f);
				var g = (byte)(_fillColor.Green * 255f);
				var b = (byte)(_fillColor.Blue * 255f);
				var a = (byte)(_fillColor.Alpha * 255f * Alpha);

				paint.Color = new SKColor(r, g, b, a);
				return paint;
			}
		}

		public void SetFillPaintShader(SKShader shader)
		{
			FillPaint.Shader = shader;
		}

		public void SetFillPaintFilterBitmap(bool value)
		{
			// todo: restore this
			//FillPaint.FilterBitmap = value;
		}

		public float ScaledStrokeSize => StrokeSize * _scaleX;

		public float ScaledFontSize => _fontSize * _scaleX;

		public new float ScaleX => _scaleX;

		public new float ScaleY => _scaleY;

		#region IDisposable Members

		public override void Dispose()
		{
			_fontPaint?.Dispose();
			_fontPaint = null;

			_strokePaint?.Dispose();
			_strokePaint = null;

			_fillPaint?.Dispose();
			_fillPaint = null;

			_shadowFilter?.Dispose();
			_shadowFilter = null;

			_blurFilter?.Dispose();
			_blurFilter = null;

			base.Dispose();
		}

		#endregion

		public void SetShadow(float blur, float sx, float sy, SKColor color)
		{
			_shadowFilter = SKImageFilter.CreateDropShadow(sx, sy, blur, blur, color);
			FillPaint.ImageFilter = _shadowFilter;
			StrokePaint.ImageFilter = _shadowFilter;
			FontPaint.ImageFilter = _shadowFilter;

			_shadowed = true;
			_shadowBlur = blur;
			_shadowX = sx;
			_shadowY = sy;
			_shadowColor = color;
		}

		public SKPaint GetShadowPaint(float sx, float sy)
		{
			if (_shadowed)
			{
				var shadowPaint = new SKPaint
				{
					Color = SKColors.Black,
					IsStroke = false,
					IsAntialias = true
				};

				// todo: implement me
				shadowPaint.ImageFilter = _shadowFilter;
				//shadowPaint.SetShadowLayer(shadowBlur, shadowX * sx, shadowY * sy, shadowColor);
				//shadowPaint.Alpha = (int) (Alpha*255f);
				return shadowPaint;
			}

			return null;
		}

		public SKPaint GetImagePaint(float sx, float sy)
		{
			var imagePaint = new SKPaint
			{
				Color = SKColors.Black,
				IsStroke = false,
				IsAntialias = true
			};

			if (Alpha < 1)
			{
				var color = new SKColor(255, 255, 255, (byte)(Alpha * 255f));
				imagePaint.ColorFilter = SKColorFilter.CreateBlendMode(color, SKBlendMode.Modulate);
			}

			if (_isBlurred)
				imagePaint.MaskFilter = _blurFilter;

			return imagePaint;
		}

		public void SetScale(float sx, float sy)
		{
			_scaleX = _scaleX * sx;
			_scaleY = _scaleY * sy;

			StrokePaint.StrokeWidth = StrokeSize * _scaleX;
#pragma warning disable CS0618 // Type or member is obsolete
			FontPaint.TextSize = _fontSize * _scaleX;
#pragma warning restore CS0618 // Type or member is obsolete
			FontFont.Size = _fontSize * _scaleX;
		}

		[Obsolete("Use Reset(SKPaint, SKFont, SKPaint, SKPaint) instead")]
		public void Reset(SKPaint fontPaint, SKPaint fillPaint, SKPaint strokePaint)
		{
			Reset(fontPaint, fontPaint?.ToFont(), fillPaint, strokePaint);
		}

		public void Reset(SKPaint fontPaint, SKFont fontFont, SKPaint fillPaint, SKPaint strokePaint)
		{
			_fontPaint?.Dispose();
			_fontPaint = fontPaint.CreateCopy();

			_fontFont?.Dispose();
			_fontFont = fontFont.CreateCopy();

			_fillPaint?.Dispose();
			_fillPaint = fillPaint.CreateCopy();

			_strokePaint?.Dispose();
			_strokePaint = strokePaint.CreateCopy();

			_shadowFilter?.Dispose();
			_shadowFilter = null;

			_blurFilter?.Dispose();
			_blurFilter = null;

			_font = null;
			_fontSize = 10f;
			Alpha = 1;
			_scaleX = 1;
			_scaleY = 1;
		}
	}
}

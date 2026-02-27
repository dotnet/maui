using Android.Graphics;
using Android.Text;
using Color = Android.Graphics.Color;

namespace Microsoft.Maui.Graphics.Platform
{
	public class PlatformCanvasState : CanvasState, IBlurrableCanvas
	{
		public float Alpha { get; set; } = 1;
		private global::Android.Graphics.Paint _fillPaint;
		private global::Android.Graphics.Paint _strokePaint;
		private IFont _font;
		private TextPaint _fontPaint;
		private float _fontSize = 10f;
		private float _scaleX = 1;
		private float _scaleY = 1;
		private bool _typefaceInvalid;
		private bool _isBlurred;
		private float _blurRadius;
		private BlurMaskFilter _blurFilter;
		private bool _shadowed;
		private global::Android.Graphics.Color _shadowColor;
		private float _shadowX;
		private float _shadowY;
		private float _shadowBlur;

		private Color _strokeColor = Colors.Black;
		private Color _fillColor = Colors.White;
		private Color _fontColor = Colors.Black;

		public PlatformCanvasState()
		{
		}

		public PlatformCanvasState(PlatformCanvasState prototype) : base(prototype)
		{
			Alpha = prototype.Alpha;
			_strokeColor = prototype._strokeColor;
			_fillColor = prototype._fillColor;
			_fontColor = prototype._fontColor;

			_fontPaint = new TextPaint(prototype.FontPaint);
			_fillPaint = new global::Android.Graphics.Paint(prototype.FillPaint);
			_strokePaint = new global::Android.Graphics.Paint(prototype.StrokePaint);
			_font = prototype._font;
			_fontSize = prototype._fontSize;
			_scaleX = prototype._scaleX;
			_scaleY = prototype._scaleY;
			_typefaceInvalid = false;

			_isBlurred = prototype._isBlurred;
			_blurRadius = prototype._blurRadius;

			_shadowed = prototype._shadowed;
			_shadowColor = prototype._shadowColor;
			_shadowX = prototype._shadowX;
			_shadowY = prototype._shadowY;
			_shadowBlur = prototype._shadowBlur;
		}

		public Color StrokeColor
		{
			get => _strokeColor;
			set => _strokeColor = value;
		}

		public Color FillColor
		{
			get => _fillColor;
			set => _fillColor = value;
		}

		public Color FontColor
		{
			get => _fontColor;
			set
			{
				_fontColor = value;
				FontPaint.Color = value != null ? _fontColor.AsColor() : global::Android.Graphics.Color.Black;
			}
		}

		public LineCap StrokeLineCap
		{
			set
			{
				if (value == LineCap.Butt)
					StrokePaint.StrokeCap = global::Android.Graphics.Paint.Cap.Butt;
				else if (value == LineCap.Round)
					StrokePaint.StrokeCap = global::Android.Graphics.Paint.Cap.Round;
				else if (value == LineCap.Square)
					StrokePaint.StrokeCap = global::Android.Graphics.Paint.Cap.Square;
			}
		}

		public LineJoin StrokeLineJoin
		{
			set
			{
				if (value == LineJoin.Miter)
					StrokePaint.StrokeJoin = global::Android.Graphics.Paint.Join.Miter;
				else if (value == LineJoin.Round)
					StrokePaint.StrokeJoin = global::Android.Graphics.Paint.Join.Round;
				else if (value == LineJoin.Bevel)
					StrokePaint.StrokeJoin = global::Android.Graphics.Paint.Join.Bevel;
			}
		}

		public float MiterLimit
		{
			set => StrokePaint.StrokeMiter = value;
		}

		public void SetStrokeDashPattern(float[] pattern, float strokeDashOffset, float strokeSize)
		{
			if (pattern == null || pattern.Length == 0 || strokeSize == 0)
			{
				StrokePaint.SetPathEffect(null);
			}
			else
			{
				float scaledStrokeSize = strokeSize * ScaleX;

				if (scaledStrokeSize == 1)
				{
					StrokePaint.SetPathEffect(new DashPathEffect(pattern, strokeDashOffset));
				}
				else
				{
					var scaledPattern = new float[pattern.Length];
					for (int i = 0; i < pattern.Length; i++)
						scaledPattern[i] = pattern[i] * scaledStrokeSize;

					var scaledStrokeDashOffset = strokeDashOffset * scaledStrokeSize;
					StrokePaint.SetPathEffect(new DashPathEffect(scaledPattern, scaledStrokeDashOffset));
				}
			}
		}

		public bool AntiAlias
		{
			set => StrokePaint.AntiAlias = value;
		}

		public bool IsBlurred => _isBlurred;

		public float BlurRadius => _blurRadius;

		public void SetBlur(float aRadius)
		{
			if (aRadius != _blurRadius)
			{
				_blurFilter?.Dispose();
				_blurFilter = null;

				if (aRadius > 0)
				{
					_isBlurred = true;
					_blurRadius = aRadius;

					_blurFilter = new BlurMaskFilter(_blurRadius, BlurMaskFilter.Blur.Normal);

					_fillPaint?.SetMaskFilter(_blurFilter);
					_strokePaint?.SetMaskFilter(_blurFilter);
					_fontPaint?.SetMaskFilter(_blurFilter);
				}
				else
				{
					_isBlurred = false;
					_blurRadius = 0;

					_fillPaint?.SetMaskFilter(null);
					_strokePaint?.SetMaskFilter(null);
					_fontPaint?.SetMaskFilter(null);
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
				FontPaint.TextSize = _fontSize * _scaleX;
			}
		}

		public IFont Font
		{
			set
			{
				if (_font != value)
				{
					_font = value;
					_typefaceInvalid = true;
				}
			}

			get => _font;
		}

		public TextPaint FontPaint
		{
			get
			{
				if (_fontPaint == null)
				{
					_fontPaint = new TextPaint();
					_fontPaint.SetARGB(1, 0, 0, 0);
					_fontPaint.AntiAlias = true;
					_fontPaint.SetTypeface(Typeface.Default);
				}

				if (_typefaceInvalid)
				{
					_fontPaint.SetTypeface(_font?.ToTypeface() ?? Typeface.Default);
					_typefaceInvalid = false;
				}

				return _fontPaint;
			}

			set => _fontPaint = value;
		}

		public global::Android.Graphics.Paint FillPaint
		{
			private get
			{
				if (_fillPaint == null)
				{
					_fillPaint = new global::Android.Graphics.Paint();
					_fillPaint.SetARGB(1, 1, 1, 1);
					_fillPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
					_fillPaint.AntiAlias = true;
				}

				return _fillPaint;
			}

			set { _fillPaint = value; }
		}

		public global::Android.Graphics.Paint StrokePaint
		{
			private get
			{
				if (_strokePaint == null)
				{
					var paint = new global::Android.Graphics.Paint();
					paint.SetARGB(1, 0, 0, 0);
					paint.StrokeWidth = 1;
					paint.StrokeMiter = CanvasDefaults.DefaultMiterLimit;
					paint.SetStyle(global::Android.Graphics.Paint.Style.Stroke);
					paint.AntiAlias = true;

					_strokePaint = paint;

					return paint;
				}

				return _strokePaint;
			}

			set { _strokePaint = value; }
		}

		public global::Android.Graphics.Paint StrokePaintWithAlpha
		{
			get
			{
				var paint = StrokePaint;

				var r = (int)(_strokeColor.Red * 255f);
				var g = (int)(_strokeColor.Green * 255f);
				var b = (int)(_strokeColor.Blue * 255f);
				var a = (int)(_strokeColor.Alpha * 255f * Alpha);

				paint.SetARGB(a, r, g, b);
				return paint;
			}
		}

		public global::Android.Graphics.Paint FillPaintWithAlpha
		{
			get
			{
				var paint = FillPaint;

				var r = (int)(_fillColor.Red * 255f);
				var g = (int)(_fillColor.Green * 255f);
				var b = (int)(_fillColor.Blue * 255f);
				var a = (int)(_fillColor.Alpha * 255f * Alpha);

				paint.SetARGB(a, r, g, b);
				return paint;
			}
		}

		public void SetFillPaintShader(Shader shader)
		{
			FillPaint.SetShader(shader);
		}

		public void SetFillPaintFilterBitmap(bool value)
		{
			FillPaint.FilterBitmap = value;
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

			base.Dispose();
		}

		#endregion

		public void SetShadow(float blur, float sx, float sy, global::Android.Graphics.Color color)
		{
			FillPaint.SetShadowLayer(blur, sx, sy, color);
			StrokePaint.SetShadowLayer(blur, sx, sy, color);
			FontPaint.SetShadowLayer(blur, sx, sy, color);

			_shadowed = true;
			_shadowBlur = blur;
			_shadowX = sx;
			_shadowY = sy;
			_shadowColor = color;
		}

		public global::Android.Graphics.Paint GetShadowPaint(float sx, float sy)
		{
			if (_shadowed)
			{
				var shadowPaint = new global::Android.Graphics.Paint();
				shadowPaint.SetARGB(255, 0, 0, 0);
				shadowPaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
				shadowPaint.AntiAlias = true;
				shadowPaint.SetShadowLayer(_shadowBlur, _shadowX * sx, _shadowY * sy, _shadowColor);
				shadowPaint.Alpha = (int)(Alpha * 255f);
				return shadowPaint;
			}

			return null;
		}

		public global::Android.Graphics.Paint GetImagePaint(float sx, float sy)
		{
			var imagePaint = GetShadowPaint(sx, sy);
			if (imagePaint == null && Alpha < 1)
			{
				imagePaint = new global::Android.Graphics.Paint();
				imagePaint.SetARGB(255, 0, 0, 0);
				imagePaint.SetStyle(global::Android.Graphics.Paint.Style.Fill);
				imagePaint.AntiAlias = true;
				imagePaint.Alpha = (int)(Alpha * 255f);
			}

			return imagePaint;
		}

		public void SetScale(float sx, float sy)
		{
			_scaleX = _scaleX * sx;
			_scaleY = _scaleY * sy;

			StrokePaint.StrokeWidth = StrokeSize * _scaleX;
			FontPaint.TextSize = _fontSize * _scaleX;
		}

		public void Reset(global::Android.Graphics.Paint aFontPaint, global::Android.Graphics.Paint aFillPaint, global::Android.Graphics.Paint aStrokePaint)
		{
			_fontPaint?.Dispose();
			_fontPaint = new TextPaint(aFontPaint);

			_fillPaint?.Dispose();
			_fillPaint = new global::Android.Graphics.Paint(aFillPaint);

			_strokePaint?.Dispose();
			_strokePaint = new global::Android.Graphics.Paint(aStrokePaint);

			_font = null;
			_fontSize = 10f;
			Alpha = 1;
			_scaleX = 1;
			_scaleY = 1;
		}
	}
}

using System;
using System.Linq;
using Android.Graphics;
using Android.Graphics.Drawables;
using AColor = Android.Graphics.Color;
using APath = Android.Graphics.Path;

namespace Xamarin.Forms.Platform.Android
{
	internal class BorderDrawable : Drawable
	{
		public const int DefaultCornerRadius = 2; // Default value for Android material button.

		readonly Func<double, float> _convertToPixels;
		bool _isDisposed;
		Bitmap _normalBitmap;
		bool _pressed;
		Bitmap _pressedBitmap;
		float _paddingLeft;
		float _paddingTop;
		Color _defaultColor;
		readonly bool _drawOutlineWithBackground;
		AColor _shadowColor;
		float _shadowDx;
		float _shadowDy;
		float _shadowRadius;

		float PaddingLeft
		{
			get { return (_paddingLeft / 2f) + _shadowDx; }
			set { _paddingLeft = value; }
		}

		float PaddingTop
		{
			get { return (_paddingTop / 2f) + _shadowDy; }
			set { _paddingTop = value; }
		}

		public BorderDrawable(Func<double, float> convertToPixels, Color defaultColor, bool drawOutlineWithBackground)
		{
			_convertToPixels = convertToPixels;
			_pressed = false;
			_defaultColor = defaultColor;
			_drawOutlineWithBackground = drawOutlineWithBackground;
		}

		public IBorderElement BorderElement
		{
			get;
			set;
		}

		public override bool IsStateful
		{
			get { return true; }
		}

		public override int Opacity
		{
			get { return 0; }
		}

		public override void Draw(Canvas canvas)
		{
			//Bounds = new Rect(Bounds.Left, Bounds.Top, Bounds.Right + (int)_convertToPixels(10), Bounds.Bottom + (int)_convertToPixels(10));
			int width = Bounds.Width();
			int height = Bounds.Height();

			if (width <= 0 || height <= 0)
				return;

			if (_normalBitmap == null ||
				_normalBitmap?.IsDisposed() == true ||
				_pressedBitmap?.IsDisposed() == true ||
				_normalBitmap.Height != height ||
				_normalBitmap.Width != width)
				Reset();

			if (!_drawOutlineWithBackground && BorderElement.BackgroundColor == Color.Default)
				return;

			Bitmap bitmap = null;
			if (GetState().Contains(global::Android.Resource.Attribute.StatePressed))
			{
				_pressedBitmap = _pressedBitmap ?? CreateBitmap(true, width, height);
				bitmap = _pressedBitmap;
			}
			else
			{
				_normalBitmap = _normalBitmap ?? CreateBitmap(false, width, height);
				bitmap = _normalBitmap;
			}

			canvas.DrawBitmap(bitmap, 0, 0, new Paint());
		}

		public BorderDrawable SetShadow(float dy, float dx, AColor color, float radius)
		{
			_shadowDx = dx;
			_shadowDy = dy;
			_shadowColor = color;
			_shadowRadius = radius;
			return this;
		}

		public BorderDrawable SetPadding(float top, float left)
		{
			_paddingTop = top;
			_paddingLeft = left;
			return this;
		}

		public void Reset()
		{
			if (_normalBitmap != null)
			{
				if (!_normalBitmap.IsDisposed())
				{
					_normalBitmap.Recycle();
					_normalBitmap.Dispose();
				}
				_normalBitmap = null;
			}

			if (_pressedBitmap != null)
			{
				if (!_pressedBitmap.IsDisposed())
				{
					_pressedBitmap.Recycle();
					_pressedBitmap.Dispose();
				}
				_pressedBitmap = null;
			}
		}

		public override void SetAlpha(int alpha)
		{
		}

		public override void SetColorFilter(ColorFilter cf)
		{
		}

		public Color BackgroundColor => BorderElement.BackgroundColor == Color.Default ? _defaultColor : BorderElement.BackgroundColor;
		public Color PressedBackgroundColor => BackgroundColor.AddLuminosity(-.12);//<item name="highlight_alpha_material_light" format="float" type="dimen">0.12</item>

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			if (disposing)
				Reset();

			base.Dispose(disposing);
		}

		protected override bool OnStateChange(int[] state)
		{
			bool old = _pressed;
			_pressed = state.Contains(global::Android.Resource.Attribute.StatePressed);
			if (_pressed != old)
			{
				InvalidateSelf();
				return true;
			}
			return false;
		}

		Bitmap CreateBitmap(bool pressed, int width, int height)
		{
			Bitmap bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
			using (var canvas = new Canvas(bitmap))
			{
				DrawBackground(canvas, width, height, pressed);
				if (_drawOutlineWithBackground)
					DrawOutline(canvas, width, height);
			}

			return bitmap;
		}

		void DrawBackground(Canvas canvas, int width, int height, bool pressed)
		{
			var paint = new Paint { AntiAlias = true };
			var path = new APath();

			float borderRadius = ConvertCornerRadiusToPixels();

			RectF rect = new RectF(0, 0, width, height);

			rect.Inset(PaddingLeft, PaddingTop);

			path.AddRoundRect(rect, borderRadius, borderRadius, APath.Direction.Cw);

			paint.Color = pressed ? PressedBackgroundColor.ToAndroid() : BackgroundColor.ToAndroid();
			paint.SetStyle(Paint.Style.Fill);
			paint.SetShadowLayer(_shadowRadius, _shadowDx, _shadowDy, _shadowColor);

			if (BorderElement.IsBackgroundSet())
			{
				Brush background = BorderElement.Background;
				paint.UpdateBackground(background, height, width);
			}

			canvas.DrawPath(path, paint);
		}

		float ConvertCornerRadiusToPixels()
		{
			int cornerRadius = DefaultCornerRadius;

			if (BorderElement.IsCornerRadiusSet() && BorderElement.CornerRadius != (int)BorderElement.CornerRadiusDefaultValue)
				cornerRadius = BorderElement.CornerRadius;

			return _convertToPixels(cornerRadius);
		}

		public RectF GetPaddingBounds(int width, int height)
		{
			RectF rect = new RectF(0, 0, width, height);
			rect.Inset(PaddingLeft, PaddingTop);
			return rect;
		}

		public void DrawCircle(Canvas canvas, int width, int height, Action<Canvas> finishDraw)
		{
			try
			{
				var radius = (float)BorderElement.CornerRadius;
				if (radius <= 0)
				{
					finishDraw(canvas);
					return;
				}

				var borderThickness = _convertToPixels(BorderElement.BorderWidth);

				using (var path = new APath())
				{
					float borderWidth = _convertToPixels(BorderElement.BorderWidth);
					float inset = borderWidth / 2;

					// adjust border radius so outer edge of stroke is same radius as border radius of background
					float borderRadius = Math.Max(ConvertCornerRadiusToPixels() - inset, 0);

					RectF rect = new RectF(0, 0, width, height);
					rect.Inset(PaddingLeft, PaddingTop);
					path.AddRoundRect(rect, borderRadius, borderRadius, APath.Direction.Ccw);

					canvas.Save();
					canvas.ClipPath(path);

					finishDraw(canvas);
				}

				canvas.Restore();
				return;
			}
			catch (Exception ex)
			{
				Internals.Log.Warning(nameof(BorderDrawable), $"Unable to create circle image: {ex}");
			}

			finishDraw(canvas);
		}

		public void DrawOutline(Canvas canvas, int width, int height)
		{
			if (BorderElement.BorderWidth <= 0)
				return;

			using (var paint = new Paint { AntiAlias = true })
			using (var path = new APath())
			{
				float borderWidth = _convertToPixels(BorderElement.BorderWidth);
				float inset = borderWidth / 2;

				// adjust border radius so outer edge of stroke is same radius as border radius of background
				float borderRadius = Math.Max(ConvertCornerRadiusToPixels() - inset, 0);

				RectF rect = new RectF(0, 0, width, height);
				rect.Inset(inset + PaddingLeft, inset + PaddingTop);

				path.AddRoundRect(rect, borderRadius, borderRadius, APath.Direction.Cw);
				paint.StrokeWidth = borderWidth;
				paint.SetStyle(Paint.Style.Stroke);
				paint.Color = BorderElement.BorderColor.ToAndroid();

				canvas.DrawPath(path, paint);
			}
		}
	}
}
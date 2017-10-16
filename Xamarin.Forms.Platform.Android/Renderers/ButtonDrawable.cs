using System;
using System.Linq;
using Android.Graphics;
using Android.Graphics.Drawables;

namespace Xamarin.Forms.Platform.Android
{
	internal class ButtonDrawable : Drawable
	{
		readonly Func<double, float> _convertToPixels;
		bool _isDisposed;
		Bitmap _normalBitmap;
		bool _pressed;
		Bitmap _pressedBitmap;

		public ButtonDrawable(Func<double, float> convertToPixels)
		{
			_convertToPixels = convertToPixels;
			_pressed = false;
		}

		public Button Button { get; set; }

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
			int width = Bounds.Width();
			int height = Bounds.Height();

			if (width <= 0 || height <= 0)
				return;

			if (_normalBitmap == null || _normalBitmap.Height != height || _normalBitmap.Width != width)
			{
				Reset();

				_normalBitmap = CreateBitmap(false, width, height);
				_pressedBitmap = CreateBitmap(true, width, height);
			}

			Bitmap bitmap = GetState().Contains(global::Android.Resource.Attribute.StatePressed) ? _pressedBitmap : _normalBitmap;
			canvas.DrawBitmap(bitmap, 0, 0, new Paint());
		}

		public void Reset()
		{
			if (_normalBitmap != null)
			{
				_normalBitmap.Recycle();
				_normalBitmap.Dispose();
				_normalBitmap = null;
			}

			if (_pressedBitmap != null)
			{
				_pressedBitmap.Recycle();
				_pressedBitmap.Dispose();
				_pressedBitmap = null;
			}
		}

		public override void SetAlpha(int alpha)
		{
		}

		public override void SetColorFilter(ColorFilter cf)
		{
		}

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
				DrawOutline(canvas, width, height);
			}

			return bitmap;
		}

		void DrawBackground(Canvas canvas, int width, int height, bool pressed)
		{
			var paint = new Paint { AntiAlias = true };
			var path = new Path();

			float borderRadius = _convertToPixels(Button.BorderRadius);

			path.AddRoundRect(new RectF(0, 0, width, height), borderRadius, borderRadius, Path.Direction.Cw);

			paint.Color = pressed ? Button.BackgroundColor.AddLuminosity(-0.1).ToAndroid() : Button.BackgroundColor.ToAndroid();
			paint.SetStyle(Paint.Style.Fill);
			canvas.DrawPath(path, paint);
		}

		void DrawOutline(Canvas canvas, int width, int height)
		{
			if (Button.BorderWidth <= 0)
				return;

			using (var paint = new Paint { AntiAlias = true })
			using (var path = new Path())
			{
				float borderWidth = _convertToPixels(Button.BorderWidth);
				float inset = borderWidth / 2;

				// adjust border radius so outer edge of stroke is same radius as border radius of background
				float borderRadius = Math.Max(_convertToPixels(Button.BorderRadius) - inset, 0);

				path.AddRoundRect(new RectF(inset, inset, width - inset, height - inset), borderRadius, borderRadius, Path.Direction.Cw);
				paint.StrokeWidth = borderWidth;
				paint.SetStyle(Paint.Style.Stroke);
				paint.Color = Button.BorderColor.ToAndroid();

				canvas.DrawPath(path, paint);
			}
		}
	}
}
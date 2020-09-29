using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using ACanvas = Android.Graphics.Canvas;
using APath = Android.Graphics.Path;

namespace Xamarin.Forms.Platform.Android
{
	public class FrameRenderer : VisualElementRenderer<Frame>
	{
		bool _disposed;
		FrameDrawable _backgroundDrawable;
		readonly MotionEventHelper _motionEventHelper = new MotionEventHelper();

		public FrameRenderer(Context context) : base(context)
		{
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use FrameRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public FrameRenderer()
		{
			AutoPackage = false;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing && !_disposed)
			{
				_backgroundDrawable?.Dispose();
				_disposed = true;
			}
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (base.OnTouchEvent(e))
				return true;

			return _motionEventHelper.HandleMotionEvent(Parent, e);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null && e.OldElement == null)
			{
				UpdateBackground();
				_motionEventHelper.UpdateElement(e.NewElement);
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName || e.PropertyName == VisualElement.BackgroundProperty.PropertyName || e.PropertyName == Frame.CornerRadiusProperty.PropertyName)
			{
				UpdateFrameBackground();
			}
		}

		void UpdateFrameBackground()
		{
			_backgroundDrawable?.Dispose();
			this.SetBackground(_backgroundDrawable = new FrameDrawable(Element, Context.ToPixels));
		}

		class FrameDrawable : Drawable
		{
			readonly Frame _frame;
			readonly Func<double, float> _convertToPixels;

			bool _isDisposed;
			Bitmap _normalBitmap;

			public FrameDrawable(Frame frame, Func<double, float> convertToPixels)
			{
				_frame = frame;
				_convertToPixels = convertToPixels;
				frame.PropertyChanged += FrameOnPropertyChanged;
			}

			public override bool IsStateful
			{
				get { return false; }
			}

			public override int Opacity
			{
				get { return 0; }
			}

			public override void Draw(ACanvas canvas)
			{
				int width = Bounds.Width();
				int height = Bounds.Height();

				if (width <= 0 || height <= 0)
				{
					if (_normalBitmap != null)
					{
						_normalBitmap.Dispose();
						_normalBitmap = null;
					}
					return;
				}

				if (_normalBitmap == null || _normalBitmap.Height != height || _normalBitmap.Width != width)
				{
					// If the user changes the orientation of the screen, make sure to destroy reference before
					// reassigning a new bitmap reference.
					if (_normalBitmap != null)
					{
						_normalBitmap.Dispose();
						_normalBitmap = null;
					}

					_normalBitmap = CreateBitmap(false, width, height);
				}
				Bitmap bitmap = _normalBitmap;
				using (var paint = new Paint())
					canvas.DrawBitmap(bitmap, 0, 0, paint);
			}

			public override void SetAlpha(int alpha)
			{
			}

			public override void SetColorFilter(ColorFilter cf)
			{
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing && !_isDisposed)
				{
					if (_normalBitmap != null)
					{
						_normalBitmap.Dispose();
						_normalBitmap = null;
					}

					_isDisposed = true;
				}

				base.Dispose(disposing);
			}

			protected override bool OnStateChange(int[] state)
			{
				return false;
			}

			Bitmap CreateBitmap(bool pressed, int width, int height)
			{
				Bitmap bitmap;
				using (Bitmap.Config config = Bitmap.Config.Argb8888)
					bitmap = Bitmap.CreateBitmap(width, height, config);

				using (var canvas = new ACanvas(bitmap))
				{
					DrawCanvas(canvas, width, height, pressed);
				}

				return bitmap;
			}

			void DrawBackground(ACanvas canvas, int width, int height, float cornerRadius, bool pressed)
			{
				using (var paint = new Paint { AntiAlias = true })
				using (var path = new APath())
				using (APath.Direction direction = APath.Direction.Cw)
				using (Paint.Style style = Paint.Style.Fill)
				using (var rect = new RectF(0, 0, width, height))
				{
					float rx = _convertToPixels(cornerRadius);
					float ry = _convertToPixels(cornerRadius);
					path.AddRoundRect(rect, rx, ry, direction);

					paint.SetStyle(style);

					if (!Brush.IsNullOrEmpty(_frame.Background))
					{
						Brush background = _frame.Background;
						paint.UpdateBackground(background, height, width);
					}
					else
					{
						global::Android.Graphics.Color color = _frame.BackgroundColor.ToAndroid();
						paint.Color = color;
					}

					canvas.DrawPath(path, paint);
				}
			}

			void DrawOutline(ACanvas canvas, int width, int height, float cornerRadius)
			{
				using (var paint = new Paint { AntiAlias = true })
				using (var path = new APath())
				using (APath.Direction direction = APath.Direction.Cw)
				using (Paint.Style style = Paint.Style.Stroke)
				using (var rect = new RectF(0, 0, width, height))
				{
					float rx = _convertToPixels(cornerRadius);
					float ry = _convertToPixels(cornerRadius);
					path.AddRoundRect(rect, rx, ry, direction);

					paint.StrokeWidth = 1;
					paint.SetStyle(style);
					paint.Color = _frame.BorderColor.ToAndroid();

					canvas.DrawPath(path, paint);
				}
			}

			void FrameOnPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName
					|| e.PropertyName == Frame.BorderColorProperty.PropertyName
					|| e.PropertyName == Frame.CornerRadiusProperty.PropertyName)
				{
					if (_normalBitmap == null)
						return;

					using (var canvas = new ACanvas(_normalBitmap))
					{
						int width = Bounds.Width();
						int height = Bounds.Height();
						canvas.DrawColor(global::Android.Graphics.Color.Black, PorterDuff.Mode.Clear);
						DrawCanvas(canvas, width, height, false);
					}
					InvalidateSelf();
				}
			}

			void DrawCanvas(ACanvas canvas, int width, int height, bool pressed)
			{
				float cornerRadius = _frame.CornerRadius;

				if (cornerRadius == -1f)
					cornerRadius = 5f; // default corner radius

				DrawBackground(canvas, width, height, cornerRadius, pressed);
				DrawOutline(canvas, width, height, cornerRadius);
			}
		}
	}
}
using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	public class BoxRenderer : VisualElementRenderer<BoxView>
	{
		bool _disposed;
		GradientDrawable _backgroundDrawable;

		readonly MotionEventHelper _motionEventHelper = new MotionEventHelper();

		public BoxRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use BoxRenderer(Context) instead.")]
		public BoxRenderer()
		{
			AutoPackage = false;
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (base.OnTouchEvent(e))
				return true;

			return _motionEventHelper.HandleMotionEvent(Parent, e);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
		{
			base.OnElementChanged(e);

			_motionEventHelper.UpdateElement(e.NewElement);

			UpdateBackgroundColor();
			UpdateCornerRadius();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == BoxView.ColorProperty.PropertyName || e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
			else if (e.PropertyName == BoxView.CornerRadiusProperty.PropertyName)
				UpdateCornerRadius();
		}

		protected override void UpdateBackgroundColor()
		{
			Color colorToSet = Element.Color;

			if (_backgroundDrawable == null)
				_backgroundDrawable = new GradientDrawable();

			if (colorToSet != Color.Default)
				_backgroundDrawable.SetColor(colorToSet.ToAndroid());
			else
				_backgroundDrawable.SetColor(colorToSet.ToAndroid(Color.Transparent));

			this.SetBackground(_backgroundDrawable);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				if (_backgroundDrawable != null)
				{
					_backgroundDrawable.Dispose();
					_backgroundDrawable = null;
				}

				if (Element != null)
				{
					Element.PropertyChanged -= OnElementPropertyChanged;

					if (Platform.GetRenderer(Element) == this)
						Element.ClearValue(Platform.RendererProperty);
				}

			}

			base.Dispose(disposing);
		}

		void UpdateCornerRadius()
		{
			var cornerRadius = Element.CornerRadius;

			if (Background is GradientDrawable backgroundGradient)
			{
				var cornerRadii = new[]
				{
					(float)(cornerRadius.TopLeft),
					(float)(cornerRadius.TopLeft),

					(float)(cornerRadius.TopRight),
					(float)(cornerRadius.TopRight),

					(float)(cornerRadius.BottomRight),
					(float)(cornerRadius.BottomRight),

					(float)(cornerRadius.BottomLeft),
					(float)(cornerRadius.BottomLeft)
				};

				backgroundGradient.SetCornerRadii(cornerRadii);
			}
		}
	}
}
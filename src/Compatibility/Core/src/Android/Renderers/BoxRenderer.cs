using System.ComponentModel;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class BoxRenderer : VisualElementRenderer<BoxView>
	{
		bool _disposed;
		GradientDrawable _backgroundDrawable;

		readonly MotionEventHelper _motionEventHelper = new MotionEventHelper();

		public BoxRenderer(Context context) : base(context)
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

			UpdateBoxView();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.IsDisposed())
			{
				return;
			}

			base.OnElementPropertyChanged(sender, e);

			if (e.IsOneOf(VisualElement.BackgroundColorProperty, VisualElement.BackgroundProperty, BoxView.ColorProperty))
				UpdateBoxView();
			else if (e.PropertyName == BoxView.CornerRadiusProperty.PropertyName)
				UpdateCornerRadius();
		}

		protected override void UpdateBackgroundColor()
		{

		}

		protected override void UpdateBackground()
		{

		}

		void UpdateBoxView()
		{
			UpdateCornerRadius();
			UpdateBoxBackground();
		}

		void UpdateBoxBackground()
		{
			Brush brushToSet = Element.Background;

			if (!Brush.IsNullOrEmpty(brushToSet))
			{
				if (_backgroundDrawable != null)
					_backgroundDrawable.UpdateBackground(brushToSet, Height, Width);
				else
				{
					_backgroundDrawable = new GradientDrawable();
					_backgroundDrawable.UpdateBackground(brushToSet, Height, Width);
					this.SetBackground(_backgroundDrawable);
				}
			}
			else
			{
				Color colorToSet = Element.Color;

				if (colorToSet == null)
					colorToSet = Element.BackgroundColor;

				if (_backgroundDrawable != null)
				{
					if (colorToSet != null)
						_backgroundDrawable.SetColor(colorToSet.ToAndroid());
					else
						_backgroundDrawable.SetColor(colorToSet.ToAndroid(Colors.Transparent));

					this.SetBackground(_backgroundDrawable);
				}
				else
				{
					if (colorToSet == null)
						colorToSet = Element.BackgroundColor;

					SetBackgroundColor(colorToSet.ToAndroid(Colors.Transparent));
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				_backgroundDrawable?.Dispose();
				_backgroundDrawable = null;

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
			if (cornerRadius == new CornerRadius(0d))
			{
				_backgroundDrawable?.Dispose();
				_backgroundDrawable = null;
			}
			else
			{
				this.SetBackground(_backgroundDrawable = new GradientDrawable());
				if (Background is GradientDrawable backgroundGradient)
				{
					var cornerRadii = new[] {
						(float)(Context.ToPixels(cornerRadius.TopLeft)),
						(float)(Context.ToPixels(cornerRadius.TopLeft)),

						(float)(Context.ToPixels(cornerRadius.TopRight)),
						(float)(Context.ToPixels(cornerRadius.TopRight)),

						(float)(Context.ToPixels(cornerRadius.BottomRight)),
						(float)(Context.ToPixels(cornerRadius.BottomRight)),

						(float)(Context.ToPixels(cornerRadius.BottomLeft)),
						(float)(Context.ToPixels(cornerRadius.BottomLeft))
					};

					backgroundGradient.SetCornerRadii(cornerRadii);
				}
			}

			UpdateBackgroundColor();
		}

		public override SizeRequest GetDesiredSize(int widthMeasureSpec, int heightMeasureSpec)
		{
			// Creating a custom override for measuring the BoxView on Android; this reports the same default size that's 
			// specified in the old OnMeasure method. Normally we'd just do this centrally in the xplat code or override
			// GetDesiredSize in a BoxViewHandler. But BoxView is a legacy control (replaced by Shapes), so we don't want
			// to bring that into the new stuff. 

			var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
			var heightMode = MeasureSpec.GetMode(heightMeasureSpec);
			var specWidth = MeasureSpec.GetSize(widthMeasureSpec);
			var specHeight = MeasureSpec.GetSize(heightMeasureSpec);

			var elementWidthRequest = Element.WidthRequest;
			var elementHeightRequest = Element.HeightRequest;

			var widthRequest = elementWidthRequest >= 0 ? elementWidthRequest : 40;
			var heightRequest = elementHeightRequest >= 0 ? elementHeightRequest : 40;

			if (widthMode != MeasureSpecMode.Exactly && widthRequest >= 0)
			{
				if (widthMode == MeasureSpecMode.Unspecified || widthRequest <= specWidth)
				{
					var deviceWidth = (int)Context.ToPixels(widthRequest);
					widthMeasureSpec = MeasureSpec.MakeMeasureSpec(deviceWidth, MeasureSpecMode.Exactly);
				}
			}

			if (heightMode != MeasureSpecMode.Exactly && heightRequest >= 0)
			{
				if (heightMode == MeasureSpecMode.Unspecified || heightRequest <= specHeight)
				{
					var deviceheight = (int)Context.ToPixels(heightRequest);
					heightMeasureSpec = MeasureSpec.MakeMeasureSpec(deviceheight, MeasureSpecMode.Exactly);
				}
			}

			return base.GetDesiredSize(widthMeasureSpec, heightMeasureSpec);
		}
	}
}
using System.ComponentModel;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	public class BoxRenderer : VisualElementRenderer<BoxView>
	{
		public BoxRenderer()
		{
			AutoPackage = false;
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			base.OnTouchEvent(e);
			return !Element.InputTransparent;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
		{
			base.OnElementChanged(e);
			UpdateBackgroundColor();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == BoxView.ColorProperty.PropertyName || e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
		}

		void UpdateBackgroundColor()
		{
			Color colorToSet = Element.Color;

			if (colorToSet == Color.Default)
				colorToSet = Element.BackgroundColor;

			SetBackgroundColor(colorToSet.ToAndroid(Color.Transparent));
		}
	}
}
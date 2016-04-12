using System.ComponentModel;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	public class BoxRenderer : VisualElementRenderer<BoxView>
	{
		bool _isInViewCell;

		public BoxRenderer()
		{
			AutoPackage = false;
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (base.OnTouchEvent(e))
				return true;
			return !Element.InputTransparent && !_isInViewCell;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				var parent = e.NewElement.Parent;
				while (parent != null)
				{
					if (parent is ViewCell)
					{
						_isInViewCell = true;
						break;
					}
					parent = parent.Parent;
				}
			}

			UpdateBackgroundColor();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == BoxView.ColorProperty.PropertyName || e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
		}

		protected override void UpdateBackgroundColor()
		{
			Color colorToSet = Element.Color;

			if (colorToSet == Color.Default)
				colorToSet = Element.BackgroundColor;

			SetBackgroundColor(colorToSet.ToAndroid(Color.Transparent));
		}
	}
}
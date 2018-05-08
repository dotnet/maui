using System.ComponentModel;
using AppKit;
using Xamarin.Forms.Platform.macOS.Controls;

namespace Xamarin.Forms.Platform.MacOS
{
	public class BoxViewRenderer : ViewRenderer<BoxView, NSView>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var boxView = new FormsBoxView();
					SetNativeControl (boxView);
				}

				SetBackgroundColor(Element.Color);
				SetCornerRadius(Element.CornerRadius);
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == BoxView.ColorProperty.PropertyName)
				SetBackgroundColor(Element.Color);
			else if (e.PropertyName == BoxView.CornerRadiusProperty.PropertyName)
				SetCornerRadius(Element.CornerRadius);
			else if (e.PropertyName == VisualElement.IsVisibleProperty.PropertyName && Element.IsVisible)
				SetNeedsDisplayInRect(Bounds);
		}

		protected override void SetBackgroundColor (Color color)
		{
			if (Element == null || Control == null)
				return;

			(Control as FormsBoxView)?.SetColor (color.ToNSColor ());
		}

		void SetCornerRadius(CornerRadius cornerRadius)
		{
			if (Element == null)
				return;

			Control.Layer.MasksToBounds = true;

			(Control as FormsBoxView)?.SetCornerRadius ((float)cornerRadius.TopLeft, (float)cornerRadius.TopRight, (float)cornerRadius.BottomLeft, (float)cornerRadius.BottomRight);
		}
	}
}
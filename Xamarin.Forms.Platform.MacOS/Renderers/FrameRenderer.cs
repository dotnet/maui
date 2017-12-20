using System.ComponentModel;
using System.Drawing;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	public class FrameRenderer : VisualElementRenderer<Frame>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
				SetupLayer();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName ||
			    e.PropertyName == Xamarin.Forms.Frame.BorderColorProperty.PropertyName ||
				e.PropertyName == Xamarin.Forms.Frame.HasShadowProperty.PropertyName)
				SetupLayer();
		}

		void SetupLayer()
		{
			Layer.CornerRadius = 5;
			if (Element.BackgroundColor == Color.Default)
				Layer.BackgroundColor = NSColor.White.CGColor;
			else
				Layer.BackgroundColor = Element.BackgroundColor.ToCGColor();

			if (Element.HasShadow)
			{
				Layer.ShadowRadius = 5;
				Layer.ShadowColor = NSColor.Black.CGColor;
				Layer.ShadowOpacity = 0.8f;
				Layer.ShadowOffset = new SizeF();
			}
			else
				Layer.ShadowOpacity = 0;

			if (Element.BorderColor == Color.Default)
				Layer.BorderColor = NSColor.Clear.CGColor;
			else
			{
				Layer.BorderColor = Element.BorderColor.ToCGColor();
				Layer.BorderWidth = 1;
			}

			Layer.RasterizationScale = NSScreen.MainScreen.BackingScaleFactor;
			Layer.ShouldRasterize = true;
		}
	}
}
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Xamarin.Forms.Platform.macOS.Controls;

namespace Xamarin.Forms.Platform.MacOS
{
	public class BoxViewRenderer : ViewRenderer<BoxView, NSView>
	{
		CGSize _previousSize;

		public override void Layout()
		{
			base.Layout();

			if (_previousSize != Bounds.Size)
				SetBackground(Element.Background);

			_previousSize = Bounds.Size;
		}

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
				SetBackground(Element.Background);
				SetCornerRadius(Element.CornerRadius);
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == BoxView.ColorProperty.PropertyName)
				SetBackgroundColor(Element.Color);
			if (e.PropertyName == BoxView.ColorProperty.PropertyName)
				SetBackground(Element.Background);
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

		protected override void SetBackground(Brush brush)
		{
			if (Element == null)
				return;

			if (Brush.IsNullOrEmpty(brush))
				SetBackgroundColor(Element.BackgroundColor);
			else
			{
				if (brush is SolidColorBrush solidColorBrush)
					(Control as FormsBoxView)?.SetColor(solidColorBrush.Color.ToNSColor());
				else
				{
					var backgroundImage = this.GetBackgroundImage(brush);
					(Control as FormsBoxView)?.SetBrush(backgroundImage != null ? NSColor.FromPatternImage(backgroundImage) : NSColor.Clear);
				}
			}
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
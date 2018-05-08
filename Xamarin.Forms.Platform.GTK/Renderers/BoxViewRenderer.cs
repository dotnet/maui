using System.ComponentModel;
using Xamarin.Forms.Platform.GTK.Extensions;
using Xamarin.Forms.PlatformConfiguration.GTKSpecific;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
	public class BoxViewRenderer : ViewRenderer<BoxView, Controls.BoxView>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new Controls.BoxView());
				}

				SetColor(Element.Color);
				SetCornerRadius(Element.CornerRadius);
				SetSize();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == BoxView.ColorProperty.PropertyName)
				SetColor(Element.Color);
			else if (e.PropertyName == BoxView.CornerRadiusProperty.PropertyName)
				SetCornerRadius(Element.CornerRadius);
			else if (e.PropertyName ==
			  PlatformConfiguration.GTKSpecific.BoxView.HasCornerRadiusProperty.PropertyName)
				SetHasCornerRadius();
		}

		protected override void OnSizeAllocated(Gdk.Rectangle allocation)
		{
			SetSize();

			base.OnSizeAllocated(allocation);
		}

		protected override void UpdateBackgroundColor()
		{
			base.UpdateBackgroundColor();

			var backgroundColor = Element.BackgroundColor == Color.Default ? Color.Transparent.ToGtkColor() : Element.BackgroundColor.ToGtkColor();

			Control.UpdateBackgroundColor(backgroundColor);

			Container.VisibleWindow = true;
		}

		private void SetColor(Color color)
		{
			if (Element == null || Control == null)
				return;

			if (color.IsDefaultOrTransparent())
			{
				Control.ResetColor();
			}
			else
			{
				var backgroundColor = color.ToGtkColor();
				Control.UpdateColor(backgroundColor);
			}
		}

		private void SetCornerRadius(CornerRadius cornerRadius)
		{
			if (Element == null || Control == null)
				return;

			Control.UpdateBorderRadius((int)cornerRadius.TopLeft, (int)cornerRadius.TopRight, (int)cornerRadius.BottomLeft, (int)cornerRadius.BottomRight);
		}

		private void SetSize()
		{
			int height = HeightRequest;
			int width = WidthRequest;

			Control.UpdateSize(height, width);
		}

		private void SetHasCornerRadius()
		{
			var hasCornerRadius = Element.OnThisPlatform().GetHasCornerRadius();

			Control.UpdateBorderRadius();
		}
	}
}
using System.ComponentModel;
using Microsoft.Maui.Controls.Compatibility.Platform.GTK.Extensions;
using Microsoft.Maui.Controls.Compatibility.PlatformConfiguration.GTKSpecific;

namespace Microsoft.Maui.Controls.Compatibility.Platform.GTK.Renderers
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

			Control.SetBackgroundColor(Element.BackgroundColor);
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
				Control.UpdateColor(color);
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

using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Tizen.NUI;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public class FrameRenderer : LayoutRenderer
	{
		public FrameRenderer()
		{
			RegisterPropertyHandler(Frame.HasShadowProperty, UpdateShadowVisibility);
			RegisterPropertyHandler(Frame.CornerRadiusProperty, UpdateCornerRadius);
			RegisterPropertyHandler(Frame.BorderColorProperty, UpdateBorderColor);
		}

		new Frame Element => base.Element as Frame;

		protected override void OnElementChanged(ElementChangedEventArgs<Layout> e)
		{
			base.OnElementChanged(e);
			NativeView.ClippingMode = ClippingModeType.ClipChildren;
		}

		void UpdateShadowVisibility()
		{
			if (Element.HasShadow)
			{
				Color shadowColor = Colors.Gray;
				if (!Element.BackgroundColor.IsDefault() && Element.BackgroundColor.Alpha > 0)
				{
					shadowColor = shadowColor.WithAlpha(Element.BackgroundColor.Alpha);
				}

				NativeView.BoxShadow = new global::Tizen.NUI.Shadow(5, shadowColor.ToNativeNUI(), new Vector2(10, 10));
			}
			else
			{
				NativeView.BoxShadow = null;
			}
		}

		void UpdateCornerRadius(bool init)
		{
			if (init && Element.CornerRadius == -1)
				return;

			if (Element.CornerRadius == -1)
			{
				NativeView.CornerRadius = 0;
			}
			else
			{
				NativeView.CornerRadius = Forms.ConvertToScaledPixel(Element.CornerRadius);
			}
		}

		void UpdateBorderColor()
		{
			NativeView.BorderlineColor = Element.BorderColor.IsDefault() ? Colors.Transparent.ToNativeNUI() : Element.BorderColor.ToNativeNUI();
			NativeView.BorderlineWidth = 1;
		}
	}
}

using Xamarin.Forms.Platform.Tizen.Native;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen.SkiaSharp
{
	public class BoxViewRenderer : CanvasViewRenderer<BoxView, RoundRectangle>
	{
		public BoxViewRenderer()
		{
			RegisterPropertyHandler(BoxView.ColorProperty, UpdateColor);
			RegisterPropertyHandler(BoxView.CornerRadiusProperty, UpdateRadius);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
		{
			if (RealControl == null)
			{
				SetRealNativeControl(new RoundRectangle(Forms.NativeParent));
			}
			base.OnElementChanged(e);

		}

		protected override void UpdateBackgroundColor(bool initialize)
		{
			if (initialize && Element.BackgroundColor.IsDefault)
				return;

			if (Element.Color.IsDefault)
			{
				UpdateColor();
			}
		}

		protected override void UpdateLayout()
		{
			base.UpdateLayout();
			RealControl.Draw(Control.Geometry);
		}

		protected override void UpdateOpacity(bool initialize)
		{
			if (initialize && Element.Opacity == 1d)
				return;

			UpdateColor();
		}

		void UpdateRadius(bool init)
		{
			CornerRadius = Element.CornerRadius;
			int topLeft = Forms.ConvertToScaledPixel(Element.CornerRadius.TopLeft);
			int topRight = Forms.ConvertToScaledPixel(Element.CornerRadius.TopRight);
			int bottomLeft = Forms.ConvertToScaledPixel(Element.CornerRadius.BottomLeft);
			int bottomRight = Forms.ConvertToScaledPixel(Element.CornerRadius.BottomRight);

			if (!init)
			{
				RealControl.Draw();
			}
		}

		void UpdateColor()
		{
			if (Element.Color.IsDefault)
			{
				if (Element.BackgroundColor.IsDefault)
				{
					// Set to default color. (Transparent)
					RealControl.Color = EColor.Transparent;
				}
				else
				{
					// Use BackgroundColor only if color is default and background color is not default.
					RealControl.Color = Element.BackgroundColor.MultiplyAlpha(Element.Opacity).ToNative();
				}
			}
			else
			{
				// Color has higer priority than BackgroundColor.
				RealControl.Color = Element.Color.MultiplyAlpha(Element.Opacity).ToNative();
			}
		}
	}
}
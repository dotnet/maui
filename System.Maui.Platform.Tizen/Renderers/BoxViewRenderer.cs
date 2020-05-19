using System.ComponentModel;
using EColor = ElmSharp.Color;
using System.Maui.Platform.Tizen.Native;

namespace System.Maui.Platform.Tizen
{
	public class BoxViewRenderer : ViewRenderer<BoxView, RoundRectangle>
	{
		public BoxViewRenderer()
		{
			RegisterPropertyHandler(nameof(Element.CornerRadius), OnRadiusUpdate);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
		{
			if (Control == null)
			{
				SetNativeControl(new RoundRectangle(System.Maui.Maui.NativeParent));
			}

			UpdateColor();
			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == BoxView.ColorProperty.PropertyName)
			{
				UpdateColor();
			}
			base.OnElementPropertyChanged(sender, e);
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
			Control.Draw(Control.Geometry);
		}

		protected override void UpdateOpacity(bool initialize)
		{
			if (initialize && Element.Opacity == 1d)
				return;

			UpdateColor();
		}

		void OnRadiusUpdate(bool init)
		{
			int topLeft = System.Maui.Maui.ConvertToScaledPixel(Element.CornerRadius.TopLeft);
			int topRight = System.Maui.Maui.ConvertToScaledPixel(Element.CornerRadius.TopRight);
			int bottomLeft = System.Maui.Maui.ConvertToScaledPixel(Element.CornerRadius.BottomLeft);
			int bottomRight = System.Maui.Maui.ConvertToScaledPixel(Element.CornerRadius.BottomRight);
			Control.SetRadius(topLeft, topRight, bottomLeft, bottomRight);
			if (!init)
			{
				Control.Draw();
			}
		}

		void UpdateColor()
		{
			if (Element.Color.IsDefault)
			{
				if (Element.BackgroundColor.IsDefault)
				{
					// Set to default color. (Transparent)
					Control.Color = EColor.Transparent;
				}
				else
				{
					// Use BackgroundColor only if color is default and background color is not default.
					Control.Color = Element.BackgroundColor.MultiplyAlpha(Element.Opacity).ToNative();
				}
			}
			else
			{
				// Color has higer priority than BackgroundColor.
				Control.Color = Element.Color.MultiplyAlpha(Element.Opacity).ToNative();
			}
		}
	}
}
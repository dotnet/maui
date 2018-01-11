using System.ComponentModel;
using EColor = ElmSharp.Color;
using ERectangle = ElmSharp.Rectangle;

namespace Xamarin.Forms.Platform.Tizen
{
	public class BoxViewRenderer : ViewRenderer<BoxView, ERectangle>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
		{
			if (Control == null)
			{
				SetNativeControl(new ERectangle(Forms.NativeParent));
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

		protected override void UpdateOpacity(bool initialize)
		{
			if (initialize && Element.Opacity == 1d)
				return;

			UpdateColor();
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
using System.ComponentModel;
using Microsoft.Maui.Controls.Compatibility.Platform.GTK.Extensions;

namespace Microsoft.Maui.Controls.Compatibility.Platform.GTK.Renderers
{
	public class FrameRenderer : ViewRenderer<Frame, Controls.CustomFrame>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					// Draw a rectangle using Cairo.
					var customFrame = new Controls.CustomFrame();
					customFrame.SetBorderWidth(2);
					customFrame.SetShadowWidth(2);

					SetNativeControl(customFrame);
				}

				PackChild();
				SetupLayer();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName.Equals("Content", System.StringComparison.InvariantCultureIgnoreCase))
			{
				PackChild();
			}
			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName ||
				e.PropertyName == Frame.BorderColorProperty.PropertyName ||
				e.PropertyName == Frame.HasShadowProperty.PropertyName)
				SetupLayer();
		}

		private void SetupLayer()
		{
			if (Element.BackgroundColor == Color.Default)
				Control.ResetBackgroundColor();
			else
				Control.SetBackgroundColor(Element.BackgroundColor);

			if (Element.BorderColor == Color.Default)
				Control.ResetBorderColor();
			else
				Control.SetBorderColor(Element.BorderColor);
		}

		private void PackChild()
		{
			if (Element.Content == null)
				return;

			IVisualElementRenderer renderer = Element.Content.GetOrCreateRenderer();
			var wrappingFixed = new Gtk.Fixed { renderer.Container };
			Control.Child = wrappingFixed;
			wrappingFixed.ShowAll();
		}
	}
}

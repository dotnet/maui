using System.ComponentModel;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
    public class FrameRenderer : ViewRenderer<Frame, Controls.CustomFrame>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
        {
            base.OnElementChanged(e);

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
                Control.SetBackgroundColor(Element.BackgroundColor.ToGtkColor());

            if (Element.BorderColor == Color.Default)
                Control.ResetBorderColor();
            else
                Control.SetBorderColor(Element.BorderColor.ToGtkColor());

            if (Element.HasShadow)
                Control.SetShadow();
            else
                Control.ResetShadow();
        }

        private void PackChild()
        {
            if (Element.Content == null)
                return;

            IVisualElementRenderer renderer = Element.Content.GetOrCreateRenderer();
            Control.Child = renderer.Container;
            renderer.Container.ShowAll();
        }
    }
}
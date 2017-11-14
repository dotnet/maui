using System.ComponentModel;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
    public class ActivityIndicatorRenderer : ViewRenderer<ActivityIndicator, Controls.ActivityIndicator>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ActivityIndicator> e)
        {
            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    Controls.ActivityIndicator activityIndicator = new Controls.ActivityIndicator();

                    SetNativeControl(activityIndicator);
                }

                UpdateColor();
                UpdateIsRunning();
            }

            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == ActivityIndicator.ColorProperty.PropertyName)
                UpdateColor();
            else if (e.PropertyName == ActivityIndicator.IsRunningProperty.PropertyName)
                UpdateIsRunning();
        }

        protected override void UpdateBackgroundColor()
        {
            base.UpdateBackgroundColor();
     
            var backgroundColor = Element.BackgroundColor == Color.Default ? Color.Transparent.ToGtkColor() : Element.BackgroundColor.ToGtkColor();

            Control.UpdateAlpha(Element.BackgroundColor == Color.Default ? 0.0 : 1.0);
            Control.UpdateBackgroundColor(backgroundColor);

            Container.VisibleWindow = true;
        }

        private void UpdateColor()
        {
            if (Element == null || Control == null)
                return;

            var color = Element.Color == Color.Default ? Color.Default.ToGtkColor() : Element.Color.ToGtkColor();

            Control.UpdateColor(color);
        }

        private void UpdateIsRunning()
        {
            if (Element == null || Control == null)
                return;

            if (Element.IsRunning)
                Control.Start();
            else
                Control.Stop();
        }
    }
}

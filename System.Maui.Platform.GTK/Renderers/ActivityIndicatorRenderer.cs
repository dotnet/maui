using System.ComponentModel;
using System.Maui.Platform.GTK.Extensions;

namespace System.Maui.Platform.GTK.Renderers
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

		private void UpdateColor()
		{
			if (Element == null || Control == null)
				return;

			Control.UpdateColor(Element.Color);
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

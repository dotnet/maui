using System.ComponentModel;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class ActivityIndicatorRenderer : ViewRenderer<ActivityIndicator, System.Windows.Controls.ProgressBar>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<ActivityIndicator> e)
		{
			base.OnElementChanged(e);

			SetNativeControl(new System.Windows.Controls.ProgressBar());

			Control.IsIndeterminate = Element.IsRunning;
			UpdateColor();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ActivityIndicator.IsRunningProperty.PropertyName)
				Control.IsIndeterminate = Element.IsRunning;
			else if (e.PropertyName == ActivityIndicator.ColorProperty.PropertyName)
				UpdateColor();
		}

		void UpdateColor()
		{
			Color color = Element.Color;
			if (color == Color.Default)
				Control.ClearValue(System.Windows.Controls.Control.ForegroundProperty);
			else
				Control.Foreground = color.ToBrush();
		}
	}
}
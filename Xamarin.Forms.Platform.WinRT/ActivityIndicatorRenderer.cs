using System.ComponentModel;
using Windows.UI.Xaml;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class ActivityIndicatorRenderer : ViewRenderer<ActivityIndicator, Windows.UI.Xaml.Controls.ProgressBar>
	{
		object _foregroundDefault;

		protected override void OnElementChanged(ElementChangedEventArgs<ActivityIndicator> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new Windows.UI.Xaml.Controls.ProgressBar { IsIndeterminate = true });

					Control.Loaded += OnControlLoaded;
				}

				// UpdateColor() called when loaded to ensure we can cache dynamic default colors
				UpdateIsRunning();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ActivityIndicator.IsRunningProperty.PropertyName)
				UpdateIsRunning();
			else if (e.PropertyName == ActivityIndicator.ColorProperty.PropertyName)
				UpdateColor();
		}

		void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			_foregroundDefault = Control.GetForegroundCache();
			UpdateColor();
		}

		void UpdateColor()
		{
			Color color = Element.Color;
			if (color.IsDefault)
			{
				Control.RestoreForegroundCache(_foregroundDefault);
			}
			else
			{
				Control.Foreground = color.ToBrush();
			}
		}

		void UpdateIsRunning()
		{
			Opacity = Element.IsRunning ? 1 : 0;
		}
	}
}
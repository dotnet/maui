using System;
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
#if !WINDOWS_UWP
		Windows.UI.Xaml.Media.SolidColorBrush _resourceBrush;
#endif
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
#if !WINDOWS_UWP
			_resourceBrush = (Control.Resources["ProgressBarIndeterminateForegroundThemeBrush"] as Windows.UI.Xaml.Media.SolidColorBrush);
			_foregroundDefault = _resourceBrush.Color;
#else
			_foregroundDefault = Control.GetForegroundCache();
#endif
			UpdateColor();
		}

		void UpdateColor()
		{
			Color color = Element.Color;

			if (color.IsDefault)
			{
#if !WINDOWS_UWP
				_resourceBrush.Color = (Windows.UI.Color) _foregroundDefault;
#else
				Control.RestoreForegroundCache(_foregroundDefault);
#endif
			}
			else
			{
#if !WINDOWS_UWP
				_resourceBrush.Color = color.ToWindowsColor();
#else
				Control.Foreground = color.ToBrush();
#endif
			}
		}

		void UpdateIsRunning()
		{
			Opacity = Element.IsRunning ? 1 : 0;
		}
	}
}
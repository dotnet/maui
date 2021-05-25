using System.ComponentModel;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class ActivityIndicatorRenderer : ViewRenderer<ActivityIndicator, FormsProgressBar>
	{
		object _foregroundDefault;

		protected override void OnElementChanged(ElementChangedEventArgs<ActivityIndicator> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new FormsProgressBar { IsIndeterminate = true,
						// TODO WINUI for some reason FormsProgressBarStyle won't load
						//Style = Microsoft.UI.Xaml.Application.Current.Resources["FormsProgressBarStyle"] as Microsoft.UI.Xaml.Style 
					});

					Control.Loaded += OnControlLoaded;
				}

				// UpdateColor() called when loaded to ensure we can cache dynamic default colors
				UpdateIsRunning();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ActivityIndicator.IsRunningProperty.PropertyName || e.PropertyName == VisualElement.OpacityProperty.PropertyName)
				UpdateIsRunning();
			else if (e.PropertyName == ActivityIndicator.ColorProperty.PropertyName)
				UpdateColor();
		}

		void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			_foregroundDefault = Control.GetForegroundCache();
			UpdateColor();
		}

		[PortHandler]
		void UpdateColor()
		{
			Color color = Element.Color;

			if (color.IsDefault())
			{
				Control.RestoreForegroundCache(_foregroundDefault);
			}
			else
			{
				Control.Foreground = Maui.ColorExtensions.ToNative(color);
			}
		}

		[PortHandler]
		void UpdateIsRunning()
		{
			Control.ElementOpacity = Element.IsRunning ? Element.Opacity : 0;
		}
	}
}
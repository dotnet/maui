using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class SwitchRenderer : ViewRenderer<Switch, ToggleSwitch>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var control = new ToggleSwitch();
					control.Toggled += OnNativeToggled;
					control.ClearValue(ToggleSwitch.OnContentProperty);
					control.ClearValue(ToggleSwitch.OffContentProperty);

					SetNativeControl(control);
				}

				Control.IsOn = Element.IsToggled;
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Switch.IsToggledProperty.PropertyName)
			{
				Control.IsOn = Element.IsToggled;
			}
		}

		protected override bool PreventGestureBubbling { get; set; } = true;

		void OnNativeToggled(object sender, RoutedEventArgs routedEventArgs)
		{
			((IElementController)Element).SetValueFromRenderer(Switch.IsToggledProperty, Control.IsOn);
		}
	}
}
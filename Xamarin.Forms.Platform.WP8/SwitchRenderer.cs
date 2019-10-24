using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls.Primitives;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class SwitchRenderer : ViewRenderer<Switch, Border>
	{
		readonly ToggleSwitchButton _toggleSwitch = new ToggleSwitchButton();

		IElementController ElementController => Element as IElementController;

		protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
		{
			base.OnElementChanged(e);

			var container = new Border { Child = _toggleSwitch };
			_toggleSwitch.IsChecked = Element.IsToggled;
			_toggleSwitch.Checked += (sender, args) => ElementController.SetValueFromRenderer(Switch.IsToggledProperty, true);
			_toggleSwitch.Unchecked += (sender, args) => ElementController.SetValueFromRenderer(Switch.IsToggledProperty, false);
			_toggleSwitch.HorizontalAlignment = HorizontalAlignment.Right;

			SetNativeControl(container);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Switch.IsToggledProperty.PropertyName)
			{
				if (_toggleSwitch.IsChecked != Element.IsToggled)
					_toggleSwitch.IsChecked = Element.IsToggled;
			}
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateSwitchIsEnabled();
		}

		protected override void UpdateNativeWidget()
		{
			base.UpdateNativeWidget();

			UpdateSwitchIsEnabled();
		}

		void UpdateSwitchIsEnabled()
		{
			_toggleSwitch.IsEnabled = Element.IsEnabled;
		}
	}
}
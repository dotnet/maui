using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class FormsTimePicker : Windows.UI.Xaml.Controls.TimePicker
	{
		public FormsTimePicker()
		{
			if (Device.Idiom == TargetIdiom.Desktop || Device.Idiom == TargetIdiom.Tablet)
			{
				Loaded += (sender, args) => { Window.Current.Activated += WindowOnActivated; };
				Unloaded += (sender, args) => { Window.Current.Activated -= WindowOnActivated; };
			}
		}

		public event EventHandler<EventArgs> ForceInvalidate;

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (Device.Idiom == TargetIdiom.Desktop || Device.Idiom == TargetIdiom.Tablet)
			{
				// Look for the combo boxes which make up a TimePicker on Windows 8.1
				// So we can hook into their closed events and invalidate them if necessary

				var hourPicker = GetTemplateChild("HourPicker") as ComboBox;
				if (hourPicker != null)
				{
					hourPicker.DropDownClosed += PickerOnDropDownClosed;
				}

				var minutePicker = GetTemplateChild("MinutePicker") as ComboBox;
				if (minutePicker != null)
				{
					minutePicker.DropDownClosed += PickerOnDropDownClosed;
				}

				var periodPicker = GetTemplateChild("PeriodPicker") as ComboBox;
				if (periodPicker != null)
				{
					periodPicker.DropDownClosed += PickerOnDropDownClosed;
				}
			}
		}

		void PickerOnDropDownClosed(object sender, object o)
		{
			// If the TimePicker is in a TableView or ListView and the user 
			// opens one of the dropdowns but does not actually change the value,
			// when the dropdown closes, the selected value will go blank
			// To fix this, we have to invalidate the control
			// This only applies to Windows 8.1
			ForceInvalidate?.Invoke(this, EventArgs.Empty);
		}

		void WindowOnActivated(object sender, WindowActivatedEventArgs windowActivatedEventArgs)
		{
			// If the TimePicker is in a TableView or ListView, when the application loses focus
			// the TextBlock/ComboBox controls (UWP and 8.1, respectively) which display its selected value
			// will go blank.
			// To fix this, we have to signal the renderer to invalidate if
			// Window.Activated occurs.
			ForceInvalidate?.Invoke(this, EventArgs.Empty);
		}
	}
}
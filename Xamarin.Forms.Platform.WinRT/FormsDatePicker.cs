using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class FormsDatePicker : Windows.UI.Xaml.Controls.DatePicker
	{
		public FormsDatePicker()
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
				// Look for the combo boxes which make up a DatePicker on Windows 8.1
				// So we can hook into their closed events and invalidate them if necessary

				var dayPicker = GetTemplateChild("DayPicker") as ComboBox;
				if (dayPicker != null)
				{
					dayPicker.DropDownClosed += PickerOnDropDownClosed;
				}

				var monthPicker = GetTemplateChild("MonthPicker") as ComboBox;
				if (monthPicker != null)
				{
					monthPicker.DropDownClosed += PickerOnDropDownClosed;
				}

				var yearPicker = GetTemplateChild("YearPicker") as ComboBox;
				if (yearPicker != null)
				{
					yearPicker.DropDownClosed += PickerOnDropDownClosed;
				}
			}
		}

		void PickerOnDropDownClosed(object sender, object o)
		{
			// If the DatePicker is in a TableView or ListView and the user 
			// opens one of the dropdowns but does not actually change the value,
			// when the dropdown closes, the selected value will go blank
			// To fix this, we have to invalidate the control
			// This only applies to Windows 8.1
			ForceInvalidate?.Invoke(this, EventArgs.Empty);
		}

		void WindowOnActivated(object sender, WindowActivatedEventArgs windowActivatedEventArgs)
		{
			// If the DatePicker is in a TableView or ListView, when the application loses and then regains focus
			// the TextBlock/ComboBox controls (UWP and 8.1, respectively) which display its selected value
			// will go blank.
			// To fix this, we have to signal the renderer to invalidate if
			// Window.Activated occurs.
			ForceInvalidate?.Invoke(this, EventArgs.Empty);
		}
	}
}
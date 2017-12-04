using System;
using System.Globalization;

namespace Xamarin.Forms.Platform.Tizen
{
	public class TimePickerRenderer : ViewRenderer<TimePicker, Native.EditfieldEntry>
	{
		//TODO need to add internationalization support
		const string DialogTitle = "Choose Time";

		static readonly string s_defaultFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;

		TimeSpan _time = DateTime.Now.TimeOfDay;

		public TimePickerRenderer()
		{
			RegisterPropertyHandler(TimePicker.FormatProperty, UpdateFormat);
			RegisterPropertyHandler(TimePicker.TimeProperty, UpdateTime);
			RegisterPropertyHandler(TimePicker.TextColorProperty, UpdateTextColor);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TimePicker> e)
		{
			if (Control == null)
			{
				var entry = new Native.EditfieldEntry(Forms.Context.MainWindow)
				{
					IsSingleLine = true,
					HorizontalTextAlignment = Native.TextAlignment.Center,
				};
				entry.SetVerticalTextAlignment("elm.text", 0.5);
				entry.AllowFocus(false);
				SetNativeControl(entry);
				Control.Clicked += OnClicked;
			}
			base.OnElementChanged(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.Clicked -= OnClicked;
				}
			}

			base.Dispose(disposing);
		}

		protected override Size MinimumSize()
		{
			return Control.Measure(Control.MinimumWidth, Control.MinimumHeight).ToDP();
		}

		void OnClicked(object o, EventArgs e)
		{
			// For EFL Entry, the event will occur even if it is currently disabled.
			// If the problem is resolved, no conditional statement is required.
			if (Element.IsEnabled)
			{
				Native.DateTimePickerDialog dialog = new Native.DateTimePickerDialog(Forms.Context.MainWindow)
				{
					Title = DialogTitle
				};

				dialog.InitializeTimePicker(_time, null);
				dialog.DateTimeChanged += OnDialogTimeChanged;
				dialog.Dismissed += OnDialogDismissed;
				dialog.Show();
			}
		}

		void OnDialogTimeChanged(object sender, Native.DateChangedEventArgs dcea)
		{
			Element.Time = dcea.NewDate.TimeOfDay;
			UpdateTime();
		}

		void OnDialogDismissed(object sender, EventArgs e)
		{
			var dialog = sender as Native.DateTimePickerDialog;
			dialog.DateTimeChanged -= OnDialogTimeChanged;
			dialog.Dismissed -= OnDialogDismissed;
		}

		void UpdateFormat()
		{
			UpdateTimeAndFormat();
		}

		void UpdateTextColor()
		{
			Control.TextColor = Element.TextColor.ToNative();
		}

		void UpdateTime()
		{
			_time = Element.Time;
			UpdateTimeAndFormat();
		}

		void UpdateTimeAndFormat()
		{
			// Xamarin using DateTime formatting (https://developer.xamarin.com/api/property/Xamarin.Forms.TimePicker.Format/)
			Control.Text = new DateTime(_time.Ticks).ToString(Element.Format ?? s_defaultFormat);
		}
	}
}
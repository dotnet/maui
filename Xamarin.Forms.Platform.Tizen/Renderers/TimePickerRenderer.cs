using System;
using System.Globalization;
using Xamarin.Forms.Platform.Tizen.Native;

namespace Xamarin.Forms.Platform.Tizen
{
	public class TimePickerRenderer : ViewRenderer<TimePicker, EditfieldEntry>
	{
		//TODO need to add internationalization support
		const string DialogTitle = "Choose Time";

		static readonly string s_defaultFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;

		TimeSpan _time = DateTime.Now.TimeOfDay;
		Lazy<DateTimePickerDialog<Native.TimePicker>> _lazyDialog;

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
				var entry = new Native.EditfieldEntry(Forms.NativeParent)
				{
					IsSingleLine = true,
					HorizontalTextAlignment = Native.TextAlignment.Center,
					InputPanelShowByOnDemand = true,
				};
				entry.SetVerticalTextAlignment("elm.text", 0.5);
				entry.TextBlockFocused += OnTextBlockFocused;
				SetNativeControl(entry);

				_lazyDialog = new Lazy<DateTimePickerDialog<Native.TimePicker>>(() =>
				{
					var dialog = new DateTimePickerDialog<Native.TimePicker>(Forms.NativeParent)
					{
						Title = DialogTitle
					};
					dialog.DateTimeChanged += OnDialogTimeChanged;
					return dialog;
				});
			}
			base.OnElementChanged(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.TextBlockFocused -= OnTextBlockFocused;
				}
				if (_lazyDialog.IsValueCreated)
				{
					_lazyDialog.Value.DateTimeChanged -= OnDialogTimeChanged;
					_lazyDialog.Value.Unrealize();
				}
			}

			base.Dispose(disposing);
		}

		protected override Size MinimumSize()
		{
			return Control.Measure(Control.MinimumWidth, Control.MinimumHeight).ToDP();
		}

		void OnTextBlockFocused(object o, EventArgs e)
		{
			// For EFL Entry, the event will occur even if it is currently disabled.
			// If the problem is resolved, no conditional statement is required.
			if (Element.IsEnabled)
			{
				var dialog = _lazyDialog.Value;
				dialog.DateTimePicker.Time = Element.Time;
				// You need to call Show() after ui thread occupation because of EFL problem.
				// Otherwise, the content of the popup will not receive focus.
				Device.BeginInvokeOnMainThread(() => dialog.Show());
			}
		}

		void OnDialogTimeChanged(object sender, Native.DateChangedEventArgs dcea)
		{
			Element.Time = dcea.NewDate.TimeOfDay;
			UpdateTime();
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

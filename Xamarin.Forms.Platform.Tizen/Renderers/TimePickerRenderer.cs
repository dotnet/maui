using System;
using System.Globalization;
using Xamarin.Forms.Platform.Tizen.Native;
using WatchDataTimePickerDialog = Xamarin.Forms.Platform.Tizen.Native.Watch.WatchDataTimePickerDialog;

namespace Xamarin.Forms.Platform.Tizen
{
	public class TimePickerRenderer : ViewRenderer<TimePicker, EditfieldEntry>
	{
		//TODO need to add internationalization support
		const string DialogTitle = "Choose Time";
		static readonly string s_defaultFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;

		TimeSpan _time = DateTime.Now.TimeOfDay;
		Lazy<IDateTimeDialog> _lazyDialog;

		public TimePickerRenderer()
		{
			RegisterPropertyHandler(TimePicker.FormatProperty, UpdateFormat);
			RegisterPropertyHandler(TimePicker.TimeProperty, UpdateTime);
			RegisterPropertyHandler(TimePicker.TextColorProperty, UpdateTextColor);
			RegisterPropertyHandler(TimePicker.FontAttributesProperty, UpdateFontAttributes);
			RegisterPropertyHandler(TimePicker.FontFamilyProperty, UpdateFontFamily);
			RegisterPropertyHandler(TimePicker.FontSizeProperty, UpdateFontSize);
		}

		protected virtual IDateTimeDialog CreateDialog()
		{
			if (Device.Idiom == TargetIdiom.Watch)
			{
				return new WatchDataTimePickerDialog(Forms.NativeParent);
			}
			else
			{
				return new DateTimePickerDialog(Forms.NativeParent);
			}
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

				_lazyDialog = new Lazy<IDateTimeDialog>(() => {
					var dialog = CreateDialog();
					dialog.Picker.Mode = DateTimePickerMode.Time;
					dialog.Title = DialogTitle;
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
				dialog.Picker.Time = Element.Time;
				// You need to call Show() after ui thread occupation because of EFL problem.
				// Otherwise, the content of the popup will not receive focus.
				Device.BeginInvokeOnMainThread(() => dialog.Show());
			}
		}

		void OnDialogTimeChanged(object sender, Native.DateChangedEventArgs dcea)
		{
			Element.SetValueFromRenderer(TimePicker.TimeProperty, dcea.NewDate.TimeOfDay);
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

		void UpdateFontSize()
		{
			Control.FontSize = Element.FontSize;
		}

		void UpdateFontFamily()
		{
			Control.FontFamily = Element.FontFamily.ToNativeFontFamily();
		}

		void UpdateFontAttributes()
		{
			Control.FontAttributes = Element.FontAttributes;
		}

		void UpdateTimeAndFormat()
		{
			// Xamarin using DateTime formatting (https://developer.xamarin.com/api/property/Xamarin.Forms.TimePicker.Format/)
			Control.Text = new DateTime(_time.Ticks).ToString(Element.Format ?? s_defaultFormat);
		}
	}
}

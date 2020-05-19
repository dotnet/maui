using System;
using System.Globalization;
using System.Maui.Platform.Tizen.Native;
using WatchDataTimePickerDialog = System.Maui.Platform.Tizen.Native.Watch.WatchDataTimePickerDialog;
using EEntry = ElmSharp.Entry;

namespace System.Maui.Platform.Tizen
{
	public class TimePickerRenderer : ViewRenderer<TimePicker, EEntry>
	{
		//TODO need to add internationalization support
		const string DialogTitle = "Choose Time";
		static readonly string s_defaultFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;

		Lazy<IDateTimeDialog> _lazyDialog;

		protected TimeSpan Time = DateTime.Now.TimeOfDay;

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
				return new WatchDataTimePickerDialog(System.Maui.Maui.NativeParent);
			}
			else
			{
				return new DateTimePickerDialog(System.Maui.Maui.NativeParent);
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TimePicker> e)
		{
			if (Control == null)
			{
				var entry = CreateNativeControl();
				entry.SetVerticalTextAlignment("elm.text", 0.5);
				SetNativeControl(entry);

				if (entry is IEntry ie)
				{
					ie.TextBlockFocused += OnTextBlockFocused;
				}

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

		protected virtual EEntry CreateNativeControl()
		{
			return new Native.EditfieldEntry(System.Maui.Maui.NativeParent)
			{
				IsSingleLine = true,
				HorizontalTextAlignment = Native.TextAlignment.Center,
				InputPanelShowByOnDemand = true,
				IsEditable = false
			};
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					if (Control is IEntry ie)
					{
						ie.TextBlockFocused -= OnTextBlockFocused;
					}
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
			if ( Control is IMeasurable im)
			{
				return im.Measure(Control.MinimumWidth, Control.MinimumHeight).ToDP();
			}
			else
			{
				return base.MinimumSize();
			}
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

		protected virtual void UpdateTextColor()
		{
			if (Control is IEntry ie)
			{
				ie.TextColor = Element.TextColor.ToNative();
			}
		}

		void UpdateTime()
		{
			Time = Element.Time;
			UpdateTimeAndFormat();
		}

		void UpdateFontSize()
		{
			if (Control is IEntry ie)
			{
				ie.FontSize = Element.FontSize;
			}
		}

		void UpdateFontFamily()
		{
			if (Control is IEntry ie)
			{
				ie.FontFamily = Element.FontFamily;
			}
		}

		void UpdateFontAttributes()
		{
			if (Control is IEntry ie)
			{
				ie.FontAttributes = Element.FontAttributes;
			}
		}

		protected virtual void UpdateTimeAndFormat()
		{
			// Xamarin using DateTime formatting (https://developer.xamarin.com/api/property/System.Maui.TimePicker.Format/)
			Control.Text = new DateTime(Time.Ticks).ToString(Element.Format ?? s_defaultFormat);
		}
	}
}

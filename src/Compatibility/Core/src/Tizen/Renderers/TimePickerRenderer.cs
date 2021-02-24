using System;
using System.Globalization;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Native;
using EEntry = ElmSharp.Entry;
using Specific = Microsoft.Maui.Controls.Compatibility.PlatformConfiguration.TizenSpecific.Application;
using WatchDateTimePickerDialog = Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Native.Watch.WatchDateTimePickerDialog;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
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
				return new WatchDateTimePickerDialog(Forms.NativeParent);
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
				var entry = CreateNativeControl();
				entry.SetVerticalTextAlignment(0.5);
				SetNativeControl(entry);

				if (entry is IEntry ie)
				{
					ie.TextBlockFocused += OnTextBlockFocused;
					ie.EntryLayoutFocused += OnFocused;
					ie.EntryLayoutUnfocused += OnUnfocused;
				}

				_lazyDialog = new Lazy<IDateTimeDialog>(() =>
				{
					var dialog = CreateDialog();
					dialog.Mode = DateTimePickerMode.Time;
					dialog.Title = DialogTitle;
					dialog.DateTimeChanged += OnDialogTimeChanged;
					dialog.PickerOpened += OnPickerOpened;
					dialog.PickerClosed += OnPickerClosed;
					return dialog;
				});
			}
			base.OnElementChanged(e);
		}

		protected virtual EEntry CreateNativeControl()
		{
			return new Native.EditfieldEntry(Forms.NativeParent)
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
						ie.EntryLayoutFocused -= OnFocused;
						ie.EntryLayoutUnfocused -= OnUnfocused;
					}
				}
				if (_lazyDialog.IsValueCreated)
				{
					_lazyDialog.Value.DateTimeChanged -= OnDialogTimeChanged;
					_lazyDialog.Value.PickerOpened -= OnPickerOpened;
					_lazyDialog.Value.PickerClosed -= OnPickerClosed;
					_lazyDialog.Value.Unrealize();
				}
			}

			base.Dispose(disposing);
		}

		protected override Size MinimumSize()
		{
			if (Control is IMeasurable im)
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
				dialog.DateTime -= dialog.DateTime.TimeOfDay;
				dialog.DateTime += Element.Time;

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
		protected virtual void OnPickerOpened(object sender, EventArgs args)
		{
			if (Specific.GetUseBezelInteraction(Application.Current))
			{
				// picker included in WatchDatePickedDialog has been activated, whenever the dialog is opend.
				Forms.RotaryFocusObject = Element;
				Specific.SetActiveBezelInteractionElement(Application.Current, Element);
			}
		}

		protected virtual void OnPickerClosed(object sender, EventArgs args)
		{
			if (Specific.GetUseBezelInteraction(Application.Current))
			{
				if (Forms.RotaryFocusObject == Element)
					Forms.RotaryFocusObject = null;
				if (Specific.GetActiveBezelInteractionElement(Application.Current) == Element)
					Specific.SetActiveBezelInteractionElement(Application.Current, null);
			}
		}

		protected virtual void UpdateTimeAndFormat()
		{
			// Xamarin using DateTime formatting (https://developer.xamarin.com/api/property/Microsoft.Maui.Controls.Compatibility.TimePicker.Format/)
			Control.Text = new DateTime(Time.Ticks).ToString(Element.Format ?? s_defaultFormat);
		}
	}
}

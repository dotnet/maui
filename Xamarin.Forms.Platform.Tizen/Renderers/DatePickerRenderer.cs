using System;
using Xamarin.Forms.Platform.Tizen.Native;

namespace Xamarin.Forms.Platform.Tizen
{
	public class DatePickerRenderer : ViewRenderer<DatePicker, EditfieldEntry>
	{
		//TODO need to add internationalization support
		const string DialogTitle = "Choose Date";
		Lazy<DateTimePickerDialog<Native.DatePicker>> _lazyDialog;

		public DatePickerRenderer()
		{
			RegisterPropertyHandler(DatePicker.DateProperty, UpdateDate);
			RegisterPropertyHandler(DatePicker.FormatProperty, UpdateDate);
			RegisterPropertyHandler(DatePicker.TextColorProperty, UpdateTextColor);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
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

				_lazyDialog = new Lazy<DateTimePickerDialog<Native.DatePicker>>(() =>
				{
					var dialog = new DateTimePickerDialog<Native.DatePicker>(Forms.NativeParent)
					{
						Title = DialogTitle
					};
					dialog.DateTimeChanged += OnDateTimeChanged;
					return dialog;
				});
			}
			base.OnElementChanged(e);
		}

		protected override Size MinimumSize()
		{
			return Control.Measure(Control.MinimumWidth, Control.MinimumHeight).ToDP();
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
					_lazyDialog.Value.DateTimeChanged -= OnDateTimeChanged;
					_lazyDialog.Value.Unrealize();
				}
			}
			base.Dispose(disposing);
		}

		void OnTextBlockFocused(object sender, EventArgs e)
		{
			// For EFL Entry, the event will occur even if it is currently disabled.
			// If the problem is resolved, no conditional statement is required.
			if (Element.IsEnabled)
			{
				var dialog = _lazyDialog.Value;
				dialog.DateTimePicker.Date = Element.Date;
				dialog.DateTimePicker.MaximumDate = Element.MaximumDate;
				dialog.DateTimePicker.MinimumDate = Element.MinimumDate;
				// You need to call Show() after ui thread occupation because of EFL problem.
				// Otherwise, the content of the popup will not receive focus.
				Device.BeginInvokeOnMainThread(() => dialog.Show());
			}
		}

		void OnDateTimeChanged(object sender, Native.DateChangedEventArgs dcea)
		{
			Element.Date = dcea.NewDate;
			Control.Text = dcea.NewDate.ToString(Element.Format);
		}

		void UpdateDate()
		{
			Control.Text = Element.Date.ToString(Element.Format);
		}

		void UpdateTextColor()
		{
			Control.TextColor = Element.TextColor.ToNative();
		}
	}
}

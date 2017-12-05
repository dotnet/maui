using System;

namespace Xamarin.Forms.Platform.Tizen
{
	public class DatePickerRenderer : ViewRenderer<DatePicker, Native.EditfieldEntry>
	{
		//TODO need to add internationalization support
		const string DialogTitle = "Choose Date";

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
				var entry = new Native.EditfieldEntry(Forms.Context.MainWindow)
				{
					IsSingleLine = true,
					HorizontalTextAlignment = Native.TextAlignment.Center,
				};
				entry.SetVerticalTextAlignment("elm.text", 0.5);
				entry.AllowFocus(false);
				entry.Clicked += OnEntryClicked;
				SetNativeControl(entry);
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
					Control.Clicked -= OnEntryClicked;
				}
			}
			base.Dispose(disposing);
		}

		void OnEntryClicked(object sender, EventArgs e)
		{
			// For EFL Entry, the event will occur even if it is currently disabled.
			// If the problem is resolved, no conditional statement is required.
			if (Element.IsEnabled)
			{
				Native.DateTimePickerDialog dialog = new Native.DateTimePickerDialog(Forms.Context.MainWindow)
				{
					Title = DialogTitle
				};

				dialog.InitializeDatePicker(Element.Date, Element.MinimumDate, Element.MaximumDate);
				dialog.DateTimeChanged += OnDateTimeChanged;
				dialog.Dismissed += OnDialogDismissed;
				dialog.Show();
			}
		}

		void OnDateTimeChanged(object sender, Native.DateChangedEventArgs dcea)
		{
			Element.Date = dcea.NewDate;
			Control.Text = dcea.NewDate.ToString(Element.Format);
		}

		void OnDialogDismissed(object sender, EventArgs e)
		{
			var dialog = sender as Native.DateTimePickerDialog;
			dialog.DateTimeChanged -= OnDateTimeChanged;
			dialog.Dismissed -= OnDialogDismissed;
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
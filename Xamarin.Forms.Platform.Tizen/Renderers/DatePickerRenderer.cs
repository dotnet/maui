using System;
using Xamarin.Forms.Platform.Tizen.Native;
using WatchDataTimePickerDialog = Xamarin.Forms.Platform.Tizen.Native.Watch.WatchDataTimePickerDialog;

namespace Xamarin.Forms.Platform.Tizen
{
	public class DatePickerRenderer : ViewRenderer<DatePicker, EditfieldEntry>
	{
		//TODO need to add internationalization support
		const string DialogTitle = "Choose Date";
		Lazy<IDateTimeDialog> _lazyDialog;

		public DatePickerRenderer()
		{
			RegisterPropertyHandler(DatePicker.DateProperty, UpdateDate);
			RegisterPropertyHandler(DatePicker.FormatProperty, UpdateDate);
			RegisterPropertyHandler(DatePicker.TextColorProperty, UpdateTextColor);
			RegisterPropertyHandler(DatePicker.FontAttributesProperty, UpdateFontAttributes);
			RegisterPropertyHandler(DatePicker.FontFamilyProperty, UpdateFontFamily);
			RegisterPropertyHandler(DatePicker.FontSizeProperty, UpdateFontSize);
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

				_lazyDialog = new Lazy<IDateTimeDialog>(() =>
				{
					var dialog = CreateDialog();
					dialog.Title = DialogTitle;
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
				dialog.Picker.DateTime = Element.Date;
				dialog.Picker.MaximumDateTime = Element.MaximumDate;
				dialog.Picker.MinimumDateTime = Element.MinimumDate;
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

		void UpdateFontSize()
		{
			Control.FontSize = Element.FontSize;
		}

		void UpdateFontFamily()
		{
			Control.FontFamily = Element.FontFamily;
		}

		void UpdateFontAttributes()
		{
			Control.FontAttributes = Element.FontAttributes;
		}
	}
}

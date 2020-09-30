using System;
using System.ComponentModel;
using System.Windows.Media;
using WDatePicker = System.Windows.Controls.DatePicker;

namespace Xamarin.Forms.Platform.WPF
{
	public class DatePickerRenderer : ViewRenderer<DatePicker, WDatePicker>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
		{
			if (e.OldElement != null)
			{
				if (Control != null)
				{
					Control.GotKeyboardFocus -= OnGotKeyboardFocus;
					Control.SelectedDateChanged -= OnNativeSelectedDateChanged;
				}
			}

			if (e.NewElement != null)
			{
				SetNativeControl(new WDatePicker());
				Control.GotKeyboardFocus += OnGotKeyboardFocus;
				Control.SelectedDateChanged += OnNativeSelectedDateChanged;

				// Update control properties 
				UpdateDate();
				UpdateMinimumDate();
				UpdateMaximumDate();
				UpdateTextColor();
				UpdateFontSize();
				UpdateFontFamily();
				UpdateFontAttributes();
			}

			base.OnElementChanged(e);
		}

		void OnGotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
		{
			if (!Control.IsDropDownOpen)
				Control.IsDropDownOpen = true;
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == DatePicker.DateProperty.PropertyName)
				UpdateDate();
			else if (e.PropertyName == DatePicker.MaximumDateProperty.PropertyName)
				UpdateMaximumDate();
			else if (e.PropertyName == DatePicker.MinimumDateProperty.PropertyName)
				UpdateMinimumDate();
			else if (e.PropertyName == DatePicker.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == DatePicker.FontSizeProperty.PropertyName)
				UpdateFontSize();
			else if (e.PropertyName == DatePicker.FontFamilyProperty.PropertyName)
				UpdateFontFamily();
			else if (e.PropertyName == DatePicker.FontAttributesProperty.PropertyName)
				UpdateFontAttributes();
		}

		void UpdateDate()
		{
			Control.SelectedDate = Element.Date;
		}

		void UpdateMaximumDate()
		{
			Control.DisplayDateEnd = Element.MaximumDate;
		}

		void UpdateMinimumDate()
		{
			Control.DisplayDateStart = Element.MinimumDate;
		}

		void UpdateTextColor()
		{
			Control.UpdateDependencyColor(WDatePicker.ForegroundProperty, Element.TextColor);
		}

		void UpdateFontFamily()
		{
			if (!string.IsNullOrEmpty(Element.FontFamily))
				Control.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), Element.FontFamily);
			else
				Control.FontFamily = (FontFamily)System.Windows.Application.Current.Resources["FontFamilyNormal"];
		}

		void UpdateFontSize()
		{
			Control.FontSize = Element.FontSize;
		}

		void UpdateFontAttributes()
		{
			Control.ApplyFontAttributes(Element.FontAttributes);
		}

		void OnNativeSelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (Control.SelectedDate.HasValue)
				((IElementController)Element).SetValueFromRenderer(DatePicker.DateProperty, Control.SelectedDate.Value);
		}

		bool _isDisposed;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				if (Control != null)
				{
					Control.SelectedDateChanged -= OnNativeSelectedDateChanged;
				}
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}
	}
}

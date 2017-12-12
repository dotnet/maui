using System;
using System.ComponentModel;
using AppKit;
using Foundation;

namespace Xamarin.Forms.Platform.MacOS
{
	public class TimePickerRenderer : ViewRenderer<TimePicker, NSDatePicker>
	{
		NSColor _defaultTextColor;
		NSColor _defaultBackgroundColor;
		bool _disposed;

		IElementController ElementController => Element;

		protected override void OnElementChanged(ElementChangedEventArgs<TimePicker> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new NSDatePicker
					{
						DatePickerMode = NSDatePickerMode.Single,
						TimeZone = new NSTimeZone("UTC"),
						DatePickerStyle = NSDatePickerStyle.TextFieldAndStepper,
						DatePickerElements = NSDatePickerElementFlags.HourMinuteSecond
					});

					Control.ValidateProposedDateValue += HandleValueChanged;
					_defaultTextColor = Control.TextColor;
					_defaultBackgroundColor = Control.BackgroundColor;
				}

				UpdateTime();
				UpdateFont();
				UpdateTextColor();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == TimePicker.TimeProperty.PropertyName ||
				e.PropertyName == TimePicker.FormatProperty.PropertyName)
				UpdateTime();

			if (e.PropertyName == TimePicker.TextColorProperty.PropertyName ||
				e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateTextColor();

			if (e.PropertyName == Picker.FontSizeProperty.PropertyName ||
				e.PropertyName == Picker.FontFamilyProperty.PropertyName ||
				e.PropertyName == Picker.FontAttributesProperty.PropertyName)
					UpdateFont();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				if (Control != null)
					Control.ValidateProposedDateValue -= HandleValueChanged;

				_disposed = true;
			}
			base.Dispose(disposing);
		}

		protected override void SetBackgroundColor(Color color)
		{
			base.SetBackgroundColor(color);

			if (Control == null)
				return;
			Control.BackgroundColor = color == Color.Default ? _defaultBackgroundColor : color.ToNSColor();
		}

		void HandleValueChanged(object sender, NSDatePickerValidatorEventArgs e)
		{
			ElementController?.SetValueFromRenderer(TimePicker.TimeProperty,
				Control.DateValue.ToDateTime() - new DateTime(2001, 1, 1));
		}

		void UpdateFont()
		{
			if (Control == null || Element == null)
				return;

			Control.Font = Element.ToNSFont();	
		}

		void UpdateTime()
		{
			if (Control == null || Element == null)
				return;
			var time = new DateTime(2001, 1, 1).Add(Element.Time);
			var newDate = time.ToNSDate();
			if (!Equals(Control.DateValue, newDate))
				Control.DateValue = newDate;
		}

		void UpdateTextColor()
		{
			if (Control == null || Element == null)
				return;
			var textColor = Element.TextColor;

			if (textColor.IsDefault || !Element.IsEnabled)
				Control.TextColor = _defaultTextColor;
			else
				Control.TextColor = textColor.ToNSColor();
		}
	}
}
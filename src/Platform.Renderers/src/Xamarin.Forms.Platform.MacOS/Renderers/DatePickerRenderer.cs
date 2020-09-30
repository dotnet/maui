using System.ComponentModel;
using AppKit;
using Foundation;

namespace Xamarin.Forms.Platform.MacOS
{
	public class DatePickerRenderer : ViewRenderer<DatePicker, NSDatePicker>
	{
		NSDatePicker _picker;
		NSColor _defaultTextColor;
		NSColor _defaultBackgroundColor;
		bool _disposed;

		IElementController ElementController => Element;

		protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement == null)
			{
				if (Control == null)
				{
					_picker = new FormsNSDatePicker
					{
						DatePickerMode = NSDatePickerMode.Single,
						TimeZone = new NSTimeZone("UTC"),
						DatePickerStyle = NSDatePickerStyle.TextFieldAndStepper,
						DatePickerElements = NSDatePickerElementFlags.YearMonthDateDay
					};
					_picker.ValidateProposedDateValue += HandleValueChanged;
					(_picker as FormsNSDatePicker).FocusChanged += ControlFocusChanged;
					_defaultTextColor = ColorExtensions.TextColor;
					_defaultBackgroundColor = ColorExtensions.ControlBackgroundColor;

					SetNativeControl(_picker);
				}
			}

			UpdateDateFromModel();
			UpdateMaximumDate();
			UpdateMinimumDate();
			UpdateFont();
			UpdateTextColor();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == DatePicker.DateProperty.PropertyName ||
				e.PropertyName == DatePicker.FormatProperty.PropertyName)
				UpdateDateFromModel();
			else if (e.PropertyName == DatePicker.MinimumDateProperty.PropertyName)
				UpdateMinimumDate();
			else if (e.PropertyName == DatePicker.MaximumDateProperty.PropertyName)
				UpdateMaximumDate();
			else if (e.PropertyName == DatePicker.TextColorProperty.PropertyName ||
					e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == Picker.FontSizeProperty.PropertyName ||
				e.PropertyName == Picker.FontFamilyProperty.PropertyName ||
				e.PropertyName == Picker.FontAttributesProperty.PropertyName)
				UpdateFont();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				if (_picker != null)
				{
					_picker.ValidateProposedDateValue -= HandleValueChanged;
					(_picker as FormsNSDatePicker).FocusChanged -= ControlFocusChanged;
				}

				_disposed = true;
			}
			base.Dispose(disposing);
		}

		protected override void SetBackgroundColor(Color color)
		{
			base.SetBackgroundColor(color);

			if (Control == null)
				return;

			if (color == Color.Default)
				Control.BackgroundColor = _defaultBackgroundColor;
			else
				Control.BackgroundColor = color.ToNSColor();
		}

		void ControlFocusChanged(object sender, BoolEventArgs e)
		{
			ElementController?.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, e.Value);
		}

		void HandleValueChanged(object sender, NSDatePickerValidatorEventArgs e)
		{
			if (Control == null || Element == null)
				return;

			ElementController?.SetValueFromRenderer(DatePicker.DateProperty, e.ProposedDateValue.ToDateTime().Date);
		}

		void UpdateDateFromModel()
		{
			if (Control == null || Element == null)
				return;
			if (_picker.DateValue.ToDateTime().Date != Element.Date.Date)
				_picker.DateValue = Element.Date.ToNSDate();
		}

		void UpdateFont()
		{
			if (Control == null || Element == null)
				return; 

			var newFont = Element.ToNSFont();

			// The font needs to have the default font size to avoid clipping
			var originalFontSize = (NSNumber)Control.Font.FontDescriptor.FontAttributes[NSFont.SizeAttribute];
			// Recreate the font with the default size
			newFont = NSFont.FromDescription(newFont.FontDescriptor, originalFontSize.FloatValue);

			// Apply the font 
			Control.Font = newFont;
		}

		void UpdateMaximumDate()
		{
			if (Control == null || Element == null)
				return;
			_picker.MaxDate = Element.MaximumDate.ToNSDate();
		}

		void UpdateMinimumDate()
		{
			if (Control == null || Element == null)
				return;
			_picker.MinDate = Element.MinimumDate.ToNSDate();
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

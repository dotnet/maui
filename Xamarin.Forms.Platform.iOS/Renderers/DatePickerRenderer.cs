using System;
using System.ComponentModel;
using Foundation;
using UIKit;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using RectangleF = CoreGraphics.CGRect;

namespace Xamarin.Forms.Platform.iOS
{
	internal class NoCaretField : UITextField
	{
		public NoCaretField() : base(new RectangleF())
		{
		}

		public override RectangleF GetCaretRectForPosition(UITextPosition position)
		{
			return new RectangleF();
		}
	}

	public class DatePickerRenderer : ViewRenderer<DatePicker, UITextField>
	{
		UIDatePicker _picker;
		UIColor _defaultTextColor;
		bool _disposed;
		bool _useLegacyColorManagement;

		IElementController ElementController => Element as IElementController;

		protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement == null)
				return;

			if (Control == null)
			{
				var entry = new NoCaretField { BorderStyle = UITextBorderStyle.RoundedRect };

				entry.EditingDidBegin += OnStarted;
				entry.EditingDidEnd += OnEnded;

				_picker = new UIDatePicker { Mode = UIDatePickerMode.Date, TimeZone = new NSTimeZone("UTC") };

				_picker.ValueChanged += HandleValueChanged;

				var width = UIScreen.MainScreen.Bounds.Width;
				var toolbar = new UIToolbar(new RectangleF(0, 0, width, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };
				var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
				var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (o, a) => entry.ResignFirstResponder());

				toolbar.SetItems(new[] { spacer, doneButton }, false);

				entry.InputView = _picker;
				entry.InputAccessoryView = toolbar;

				entry.InputView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
				entry.InputAccessoryView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

				_defaultTextColor = entry.TextColor;

				_useLegacyColorManagement = e.NewElement.UseLegacyColorManagement();

				SetNativeControl(entry);
			}

			UpdateDateFromModel(false);
			UpdateFont();
			UpdateMaximumDate();
			UpdateMinimumDate();
			UpdateTextColor();
			UpdateFlowDirection();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == DatePicker.DateProperty.PropertyName || e.PropertyName == DatePicker.FormatProperty.PropertyName)
				UpdateDateFromModel(true);
			else if (e.PropertyName == DatePicker.MinimumDateProperty.PropertyName)
				UpdateMinimumDate();
			else if (e.PropertyName == DatePicker.MaximumDateProperty.PropertyName)
				UpdateMaximumDate();
			else if (e.PropertyName == DatePicker.TextColorProperty.PropertyName || e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
			else if (e.PropertyName == DatePicker.FontAttributesProperty.PropertyName || e.PropertyName == DatePicker.FontFamilyProperty.PropertyName || e.PropertyName == DatePicker.FontSizeProperty.PropertyName)
				UpdateFont();
		}

		void HandleValueChanged(object sender, EventArgs e)
		{
			ElementController?.SetValueFromRenderer(DatePicker.DateProperty, _picker.Date.ToDateTime().Date);
		}

		void OnEnded(object sender, EventArgs eventArgs)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
		}

		void OnStarted(object sender, EventArgs eventArgs)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		void UpdateDateFromModel(bool animate)
		{
			if (_picker.Date.ToDateTime().Date != Element.Date.Date)
				_picker.SetDate(Element.Date.ToNSDate(), animate);

			Control.Text = Element.Date.ToString(Element.Format);
		}

		void UpdateFlowDirection()
		{
			(Control as UITextField).UpdateTextAlignment(Element);
		}
		
		void UpdateFont()
		{
			Control.Font = Element.ToUIFont();
		}

		void UpdateMaximumDate()
		{
			_picker.MaximumDate = Element.MaximumDate.ToNSDate();
		}

		void UpdateMinimumDate()
		{
			_picker.MinimumDate = Element.MinimumDate.ToNSDate();
		}

		void UpdateTextColor()
		{
			var textColor = Element.TextColor;

			if (textColor.IsDefault || (!Element.IsEnabled && _useLegacyColorManagement))
				Control.TextColor = _defaultTextColor;
			else
				Control.TextColor = textColor.ToUIColor();

			// HACK This forces the color to update; there's probably a more elegant way to make this happen
			Control.Text = Control.Text;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				_defaultTextColor = null;

				if (_picker != null)
				{
					_picker.RemoveFromSuperview();
					_picker.ValueChanged -= HandleValueChanged;
					_picker.Dispose();
					_picker = null;
				}

				if (Control != null)
				{
					Control.EditingDidBegin -= OnStarted;
					Control.EditingDidEnd -= OnEnded;
				}
			}

			base.Dispose(disposing);
		}
	}
}
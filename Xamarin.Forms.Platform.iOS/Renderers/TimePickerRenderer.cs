using System;
using System.ComponentModel;
using Foundation;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Xamarin.Forms.Platform.iOS
{
	public class TimePickerRenderer : ViewRenderer<TimePicker, UITextField>
	{
		UIDatePicker _picker;
		UIColor _defaultTextColor;
		bool _disposed;
		bool _useLegacyColorManagement;

		IElementController ElementController => Element as IElementController;

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
					_picker.ValueChanged -= OnValueChanged;
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

		protected override void OnElementChanged(ElementChangedEventArgs<TimePicker> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var entry = new NoCaretField { BorderStyle = UITextBorderStyle.RoundedRect };

					entry.EditingDidBegin += OnStarted;
					entry.EditingDidEnd += OnEnded;

					_picker = new UIDatePicker { Mode = UIDatePickerMode.Time, TimeZone = new NSTimeZone("UTC") };

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

					_picker.ValueChanged += OnValueChanged;

					SetNativeControl(entry);
				}

				UpdateFont();
				UpdateTime();
				UpdateTextColor();
				UpdateFlowDirection();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == TimePicker.TimeProperty.PropertyName || e.PropertyName == TimePicker.FormatProperty.PropertyName)
				UpdateTime();
			else if (e.PropertyName == TimePicker.TextColorProperty.PropertyName || e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == TimePicker.FontAttributesProperty.PropertyName || e.PropertyName == TimePicker.FontFamilyProperty.PropertyName || e.PropertyName == TimePicker.FontSizeProperty.PropertyName)
				UpdateFont();

			if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
		}

		void OnEnded(object sender, EventArgs eventArgs)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
		}

		void OnStarted(object sender, EventArgs eventArgs)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		void OnValueChanged(object sender, EventArgs e)
		{
			ElementController.SetValueFromRenderer(TimePicker.TimeProperty, _picker.Date.ToDateTime() - new DateTime(1, 1, 1));
		}

		void UpdateFlowDirection()
		{
			(Control as UITextField).UpdateTextAlignment(Element);
		}
		
		void UpdateFont()
		{
			Control.Font = Element.ToUIFont();
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

		void UpdateTime()
		{
			_picker.Date = new DateTime(1, 1, 1).Add(Element.Time).ToNSDate();
			Control.Text = DateTime.Today.Add(Element.Time).ToString(Element.Format);
		}
	}
}
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Media;
using Microsoft.Phone.Controls;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class DatePickerRenderer : ViewRenderer<DatePicker, Microsoft.Phone.Controls.DatePicker>
	{
		Brush _defaultBrush;

		protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
		{
			base.OnElementChanged(e);

			var datePicker = new Microsoft.Phone.Controls.DatePicker { Value = Element.Date };

			datePicker.Loaded += (sender, args) => {
				// The defaults from the control template won't be available
				// right away; we have to wait until after the template has been applied
				_defaultBrush = datePicker.Foreground;
				UpdateTextColor();
			};

			datePicker.ValueChanged += DatePickerOnValueChanged;
			SetNativeControl(datePicker);

			UpdateFormatString();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == DatePicker.DateProperty.PropertyName)
				Control.Value = Element.Date;
			else if (e.PropertyName == DatePicker.FormatProperty.PropertyName)
				UpdateFormatString();
			else if (e.PropertyName == DatePicker.TextColorProperty.PropertyName)
				UpdateTextColor();
		}

		internal override void OnModelFocusChangeRequested(object sender, VisualElement.FocusRequestArgs args)
		{
			Microsoft.Phone.Controls.DatePicker control = Control;
			if (control == null)
				return;

			if (args.Focus)
			{
				typeof(DateTimePickerBase).InvokeMember("OpenPickerPage", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, Type.DefaultBinder, control, null);
				args.Result = true;
			}
			else
			{
				UnfocusControl(control);
				args.Result = true;
			}
		}

		void DatePickerOnValueChanged(object sender, DateTimeValueChangedEventArgs dateTimeValueChangedEventArgs)
		{
			if (Control.Value.HasValue)
				((IElementController)Element).SetValueFromRenderer(DatePicker.DateProperty, Control.Value.Value);
		}

		void UpdateFormatString()
		{
			Control.ValueStringFormat = "{0:" + Element.Format + "}";
		}

		void UpdateTextColor()
		{
			Color color = Element.TextColor;
			Control.Foreground = color.IsDefault ? (_defaultBrush ?? color.ToBrush()) : color.ToBrush();
		}
	}
}
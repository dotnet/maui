using System;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Phone.Controls;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class DatePickerRenderer : ViewRenderer<DatePicker, Microsoft.Phone.Controls.DatePicker>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
		{
			base.OnElementChanged(e);

			var datePicker = new Microsoft.Phone.Controls.DatePicker { Value = Element.Date };
			datePicker.ValueChanged += DatePickerOnValueChanged;
			SetNativeControl(datePicker);

			UpdateFormatString();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Date")
				Control.Value = Element.Date;
			else if (e.PropertyName == DatePicker.FormatProperty.PropertyName)
				UpdateFormatString();
			base.OnElementPropertyChanged(sender, e);
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
	}
}
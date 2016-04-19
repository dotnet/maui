using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Media;
using Microsoft.Phone.Controls;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class TimePickerRenderer : ViewRenderer<TimePicker, Microsoft.Phone.Controls.TimePicker>
	{
		Brush _defaultBrush;

		protected override void OnElementChanged(ElementChangedEventArgs<TimePicker> e)
		{
			base.OnElementChanged(e);

			var timePicker = new Microsoft.Phone.Controls.TimePicker { Value = DateTime.Today.Add(Element.Time) };

			timePicker.Loaded += (sender, args) =>
			{
				// The defaults from the control template won't be available
				// right away; we have to wait until after the template has been applied
				_defaultBrush = timePicker.Foreground;
				UpdateTextColor();
			};

			timePicker.ValueChanged += TimePickerOnValueChanged;

			SetNativeControl(timePicker);
			UpdateFormatString();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == TimePicker.TimeProperty.PropertyName)
				Control.Value = DateTime.Today.Add(Element.Time);
			else if (e.PropertyName == TimePicker.FormatProperty.PropertyName)
				UpdateFormatString();
			else if (e.PropertyName == TimePicker.TextColorProperty.PropertyName)
				UpdateTextColor();
		}

		internal override void OnModelFocusChangeRequested(object sender, VisualElement.FocusRequestArgs args)
		{
			Microsoft.Phone.Controls.TimePicker control = Control;
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

		void TimePickerOnValueChanged(object sender, DateTimeValueChangedEventArgs dateTimeValueChangedEventArgs)
		{
			if (Control.Value != null)
				((IElementController)Element).SetValueFromRenderer(TimePicker.TimeProperty, Control.Value.Value - DateTime.Today);
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
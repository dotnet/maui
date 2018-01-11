using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WPF
{
	public class TimePickerRenderer : ViewRenderer<Xamarin.Forms.TimePicker, FormsTimePicker>
	{
		Brush _defaultBrush;
		bool _fontApplied;
		FontFamily _defaultFontFamily;

		protected override void Dispose(bool disposing)
		{
			if (disposing && Control != null)
			{
				Control.TimeChanged -= OnControlTimeChanged;
				Control.Loaded -= ControlOnLoaded;
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.TimePicker> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var picker = new FormsTimePicker();
					SetNativeControl(picker);

					Control.TimeChanged += OnControlTimeChanged;
					Control.Loaded += ControlOnLoaded;
				}

				UpdateTime();
				UpdateFlowDirection();
				UpdateTimeFormat();
			}
		}

		void ControlOnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			// The defaults from the control template won't be available
			// right away; we have to wait until after the template has been applied
			_defaultBrush = Control.Foreground;
			_defaultFontFamily = Control.FontFamily;
			UpdateFont();
			UpdateTextColor();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == TimePicker.TimeProperty.PropertyName)
				UpdateTime();
			else if (e.PropertyName == TimePicker.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == TimePicker.FontAttributesProperty.PropertyName || e.PropertyName == TimePicker.FontFamilyProperty.PropertyName || e.PropertyName == TimePicker.FontSizeProperty.PropertyName)
				UpdateFont();

			if (e.PropertyName == Xamarin.Forms.TimePicker.FormatProperty.PropertyName)
				UpdateTimeFormat();

			if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
		}

		void OnControlTimeChanged(object sender, TimeChangedEventArgs e)
		{
			Element.Time = e.NewTime.HasValue ? e.NewTime.Value : (TimeSpan)Xamarin.Forms.TimePicker.TimeProperty.DefaultValue;
			((IVisualElementController)Element)?.InvalidateMeasure(InvalidationTrigger.SizeRequestChanged);
		}

		void UpdateTimeFormat()
		{
			Control.TimeFormat = Element.Format;
		}

		void UpdateFlowDirection()
		{
			Control.FlowDirection = Element.FlowDirection == Xamarin.Forms.FlowDirection.RightToLeft ? System.Windows.FlowDirection.RightToLeft : System.Windows.FlowDirection.LeftToRight;
		}

		void PickerOnForceInvalidate(object sender, EventArgs eventArgs)
		{
			((IVisualElementController)Element)?.InvalidateMeasure(InvalidationTrigger.SizeRequestChanged);
		}

		void UpdateFont()
		{
			if (Control == null)
				return;

			TimePicker timePicker = Element;

			if (timePicker == null)
				return;

			bool timePickerIsDefault = timePicker.FontFamily == null && timePicker.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(TimePicker), true) && timePicker.FontAttributes == FontAttributes.None;

			if (timePickerIsDefault && !_fontApplied)
				return;

			if (timePickerIsDefault)
			{
				Control.ClearValue(FormsTimePicker.FontStyleProperty);
				Control.ClearValue(FormsTimePicker.FontSizeProperty);
				Control.ClearValue(FormsTimePicker.FontFamilyProperty);
				Control.ClearValue(FormsTimePicker.FontWeightProperty);
				Control.ClearValue(FormsTimePicker.FontStretchProperty);
			}
			else
			{
				Control.ApplyFont(timePicker);
			}

			_fontApplied = true;
		}

		void UpdateTime()
		{
			Control.Time = Element.Time;
		}

		void UpdateTextColor()
		{
			Color color = Element.TextColor;
			Control.Foreground = color.IsDefault ? (_defaultBrush ?? color.ToBrush()) : color.ToBrush();
		}
	}
}

using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.UWP
{
	public class DatePickerRenderer : ViewRenderer<DatePicker, Windows.UI.Xaml.Controls.DatePicker>
	{
		Brush _defaultBrush;

		protected override void Dispose(bool disposing)
		{
			if (disposing && Control != null)
			{
				Control.DateChanged -= OnControlDateChanged;
				Control.Loaded -= ControlOnLoaded;
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var picker = new Windows.UI.Xaml.Controls.DatePicker();
					SetNativeControl(picker);
					Control.Loaded += ControlOnLoaded;
					Control.DateChanged += OnControlDateChanged;
				}

				UpdateMinimumDate();
				UpdateMaximumDate();
				UpdateDate(e.NewElement.Date);
				UpdateFlowDirection();
			}

			base.OnElementChanged(e);
		}

		void ControlOnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			// The defaults from the control template won't be available
			// right away; we have to wait until after the template has been applied
			_defaultBrush = Control.Foreground;
			UpdateTextColor();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == DatePicker.DateProperty.PropertyName)
				UpdateDate(Element.Date);
			else if (e.PropertyName == DatePicker.MaximumDateProperty.PropertyName)
				UpdateMaximumDate();
			else if (e.PropertyName == DatePicker.MinimumDateProperty.PropertyName)
				UpdateMinimumDate();
			else if (e.PropertyName == DatePicker.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
		}

		protected override bool PreventGestureBubbling { get; set; } = true;

		void OnControlDateChanged(object sender, DatePickerValueChangedEventArgs e)
		{
			Element.Date = e.NewDate.Date;
			DateTime currentDate = Element.Date;
			if (currentDate != e.NewDate.Date) // Match coerced value
				UpdateDate(currentDate);

			((IVisualElementController)Element).InvalidateMeasure(InvalidationTrigger.SizeRequestChanged);
		}

		void UpdateDate(DateTime date)
		{
			Control.Date = date;
		}

		void UpdateFlowDirection()
		{
			Control.UpdateFlowDirection(Element);
		}

		void UpdateMaximumDate()
		{
			DateTime maxdate = Element.MaximumDate;
			Control.MaxYear = new DateTimeOffset(maxdate);
		}

		void UpdateMinimumDate()
		{
			DateTime mindate = Element.MinimumDate;

			try
			{
				Control.MinYear = new DateTimeOffset(mindate);
			}
			catch (ArgumentOutOfRangeException)
			{
				// This will be thrown when mindate equals DateTime.MinValue and the UTC offset is positive
				// because the resulting DateTimeOffset.UtcDateTime will be out of range. In that case let's
				// specify the Kind as UTC so there is no offset.
				mindate = DateTime.SpecifyKind(mindate, DateTimeKind.Utc);
				Control.MinYear = new DateTimeOffset(mindate);
			}
		}

		void UpdateTextColor()
		{
			Color color = Element.TextColor;
			Control.Foreground = color.IsDefault ? (_defaultBrush ?? color.ToBrush()) : color.ToBrush();
		}
	}
}
using System;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class DatePickerRenderer : ViewRenderer<DatePicker, FormsDatePicker>, IWrapperAware
	{
		public void NotifyWrapped()
		{
			if (Control != null)
			{
				Control.ForceInvalidate += PickerOnForceInvalidate;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && Control != null)
			{
				Control.ForceInvalidate -= PickerOnForceInvalidate;
				Control.DateChanged -= OnControlDateChanged;
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var picker = new FormsDatePicker();
					picker.DateChanged += OnControlDateChanged;
					SetNativeControl(picker);
				}

				UpdateMinimumDate();
				UpdateMaximumDate();
				UpdateDate(e.NewElement.Date);
			}

			base.OnElementChanged(e);
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
		}

		void OnControlDateChanged(object sender, DatePickerValueChangedEventArgs e)
		{
			Element.Date = e.NewDate.Date;
			DateTime currentDate = Element.Date;
			if (currentDate != e.NewDate.Date) // Match coerced value
				UpdateDate(currentDate);

			Element.InvalidateMeasure(InvalidationTrigger.SizeRequestChanged);
		}

		void PickerOnForceInvalidate(object sender, EventArgs eventArgs)
		{
			Element?.InvalidateMeasure(InvalidationTrigger.SizeRequestChanged);
		}

		void UpdateDate(DateTime date)
		{
			Control.Date = date;
		}

		void UpdateMaximumDate()
		{
			DateTime maxdate = Element.MaximumDate;
			Control.MaxYear = new DateTimeOffset(maxdate.Date);
		}

		void UpdateMinimumDate()
		{
			DateTime mindate = Element.MinimumDate;
			Control.MinYear = new DateTimeOffset(mindate);
		}
	}
}
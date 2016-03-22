using System;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class TimePickerRenderer : ViewRenderer<TimePicker, FormsTimePicker>, IWrapperAware
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
				Control.TimeChanged -= OnControlTimeChanged;
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TimePicker> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var picker = new FormsTimePicker();
					picker.TimeChanged += OnControlTimeChanged;
					SetNativeControl(picker);
				}

				UpdateTime();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == TimePicker.TimeProperty.PropertyName)
				UpdateTime();
		}

		void OnControlTimeChanged(object sender, TimePickerValueChangedEventArgs e)
		{
			Element.Time = e.NewTime;
			Element?.InvalidateMeasure(InvalidationTrigger.SizeRequestChanged);
		}

		void PickerOnForceInvalidate(object sender, EventArgs eventArgs)
		{
			Element?.InvalidateMeasure(InvalidationTrigger.SizeRequestChanged);
		}

		void UpdateTime()
		{
			Control.Time = Element.Time;
		}
	}
}
#nullable disable

namespace Microsoft.Maui.Controls
{
	static class PickerElement
	{
		public static readonly BindableProperty IsOpenProperty =	
			BindableProperty.Create(nameof(IPickerElement.IsOpen), typeof(bool), typeof(PickerElement), default, BindingMode.TwoWay,
				propertyChanged: OnIsOpenPropertyChanged);

		static void OnIsOpenPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((IPickerElement)bindable).OnIsOpenPropertyChanged((bool)oldValue, (bool)newValue);
		}

		static void IsOpenPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var datePicker = (DatePicker)bindable;

			bool isOpen = (bool)newValue;

			// Only process if there's an actual change
			if ((bool)oldValue == isOpen)
				return;

			if (isOpen)
				datePicker.Opened?.Invoke(datePicker, new PickerOpenedEventArgs());
			else
				datePicker.Closed?.Invoke(datePicker, new PickerClosedEventArgs());
		}
	}
}

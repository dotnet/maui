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
	}
}
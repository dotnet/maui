using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public static class RadioButtonExtensions
	{
		public static void UpdateIsChecked(this RadioButton nativeRadioButton, IRadioButton radioButton)
		{
			nativeRadioButton.IsChecked = radioButton.IsChecked;
		}
	}
}

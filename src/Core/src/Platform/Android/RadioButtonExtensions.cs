using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui
{
	public static class RadioButtonExtensions
	{
		public static void UpdateIsChecked(this AppCompatRadioButton nativeRadioButton, IRadioButton radioButton)
		{
			nativeRadioButton.Checked = radioButton.IsChecked;
		}
	}
}
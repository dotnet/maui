namespace Microsoft.Maui
{
	public static class RadioButtonExtensions
	{
		public static void UpdateIsChecked(this MauiRadioButton nativeRadioButton, IRadioButton radioButton)
		{
			nativeRadioButton.GroupValue = radioButton.IsChecked ? 1 : 0;
		}

		public static void UpdateTextColor(this MauiRadioButton nativeRadioButton, ITextStyle radioButton)
		{
			nativeRadioButton.TextColor = radioButton.TextColor.ToNative();
		}
	}
}
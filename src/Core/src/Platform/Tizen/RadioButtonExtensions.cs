namespace Microsoft.Maui
{
	public static class RadioButtonExtensions
	{
		public static void UpdateIsChecked(this MauiRadioButton platformRadioButton, IRadioButton radioButton)
		{
			platformRadioButton.GroupValue = radioButton.IsChecked ? 1 : 0;
		}

		public static void UpdateTextColor(this MauiRadioButton platformRadioButton, ITextStyle radioButton)
		{
			platformRadioButton.TextColor = radioButton.TextColor.ToPlatform();
		}
	}
}
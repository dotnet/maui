namespace Microsoft.Maui
{
	public static class CheckBoxExtensions
	{
		public static void UpdateIsChecked(this MauiCheckBox nativeCheckBox, ICheckBox check)
		{
			nativeCheckBox.IsChecked = check.IsChecked;
		}

		public static void UpdateColor(this MauiCheckBox nativeCheckBox, ICheckBox check)
		{
			nativeCheckBox.CheckBoxTintColor = check.Color;
		}
	}
}
namespace Microsoft.Maui
{
	public static class CheckBoxExtensions
	{
		public static void UpdateIsChecked(this NativeCheckBox nativeCheckBox, ICheck check)
		{
			nativeCheckBox.IsChecked = check.IsChecked;
		}

		public static void UpdateColor(this NativeCheckBox nativeCheckBox, ICheck check)
		{
			nativeCheckBox.CheckBoxTintColor = check.Color;
		}
	}
}
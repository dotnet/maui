namespace Microsoft.Maui
{
	public static class CheckBoxExtensions
	{
		public static void UpdateIsChecked(this NativeCheckBox nativeCheckBox, ICheckBox check)
		{
			nativeCheckBox.IsChecked = check.IsChecked;
		}
	}
}
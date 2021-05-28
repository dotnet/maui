namespace Microsoft.Maui
{
	public static class PickerExtensions
	{
		public static void UpdateTitle(this MauiPicker nativePicker, IPicker picker) =>
			UpdatePicker(nativePicker, picker);

		public static void UpdateSelectedIndex(this MauiPicker nativePicker, IPicker picker) =>
			UpdatePicker(nativePicker, picker);

		internal static void UpdatePicker(this MauiPicker nativePicker, IPicker picker)
		{
			nativePicker.Hint = picker.Title;

			if (picker.SelectedIndex == -1 || picker.SelectedIndex >= picker.GetCount())
				nativePicker.Text = null;
			else
				nativePicker.Text = picker.GetItem(picker.SelectedIndex);
		}
	}
}
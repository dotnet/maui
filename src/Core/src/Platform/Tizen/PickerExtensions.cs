using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui
{
	public static class PickerExtensions
	{
		public static void UpdateTitle(this Entry nativePicker, IPicker picker) =>
			UpdatePicker(nativePicker, picker);

		public static void UpdateTitleColor(this Entry nativePicker, IPicker picker) =>
			nativePicker.PlaceholderColor = picker.TitleColor.ToNative();

		public static void UpdateSelectedIndex(this Entry nativePicker, IPicker picker) =>
			UpdatePicker(nativePicker, picker);

		internal static void UpdatePicker(this Entry nativePicker, IPicker picker)
		{
			if (picker.SelectedIndex == -1 || picker.SelectedIndex >= picker.GetCount())
				nativePicker.Text = string.Empty;
			else
				nativePicker.Text = picker.GetItem(picker.SelectedIndex);
		}
	}
}
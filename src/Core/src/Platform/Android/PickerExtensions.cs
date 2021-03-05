namespace Microsoft.Maui
{
	public static class PickerExtensions
	{ 
		public static void UpdateTitle(this NativePicker nativePicker, IPicker picker) =>
			UpdatePicker(nativePicker, picker);

		public static void UpdateSelectedIndex(this NativePicker nativePicker, IPicker picker) =>
			UpdatePicker(nativePicker, picker);

		internal static void UpdatePicker(this NativePicker nativePicker, IPicker picker)
		{
			nativePicker.Hint = picker.Title;

			if (picker.SelectedIndex == -1 || picker.Items == null || picker.SelectedIndex >= picker.Items.Count)
				nativePicker.Text = null;
			else
				nativePicker.Text = picker.Items[picker.SelectedIndex];

			nativePicker.SetSelectedItem(picker);
		}

		internal static void SetSelectedItem(this NativePicker nativePicker, IPicker picker)
		{
			if (picker == null || nativePicker == null)
				return;

			int index = picker.SelectedIndex;

			if (index == -1)
			{
				picker.SelectedItem = null;
				return;
			}

			if (picker.ItemsSource != null)
			{
				picker.SelectedItem = picker.ItemsSource[index];
				return;
			}

			if (picker.Items != null)
				picker.SelectedItem = picker.Items[index];
		}
	}
}
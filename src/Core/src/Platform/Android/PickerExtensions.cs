namespace Microsoft.Maui
{
	public static class PickerExtensions
	{
		public static void UpdateTitle(this MauiPicker nativePicker, IPicker picker) =>
			UpdatePicker(nativePicker, picker);

		public static void UpdateSelectedIndex(this MauiPicker nativePicker, IPicker picker) =>
			UpdatePicker(nativePicker, picker);

		public static void UpdateCharacterSpacing(this MauiPicker nativePicker, IPicker picker)
		{
			nativePicker.LetterSpacing = picker.CharacterSpacing.ToEm();
		}

		internal static void UpdatePicker(this MauiPicker nativePicker, IPicker picker)
		{
			nativePicker.Hint = picker.Title;

			if (picker.SelectedIndex == -1 || picker.Items == null || picker.SelectedIndex >= picker.Items.Count)
				nativePicker.Text = null;
			else
				nativePicker.Text = picker.Items[picker.SelectedIndex];

			nativePicker.SetSelectedItem(picker);
		}

		internal static void SetSelectedItem(this MauiPicker nativePicker, IPicker picker)
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
using Android.Content.Res;

namespace Microsoft.Maui
{
	public static class PickerExtensions
	{
		static readonly int[][] ColorStates =
		{
 			new[] { Android.Resource.Attribute.StateEnabled },
 			new[] { -Android.Resource.Attribute.StateEnabled }
 		};

		public static void UpdateTitle(this MauiPicker nativePicker, IPicker picker) =>
			UpdatePicker(nativePicker, picker);

		public static void UpdateTitleColor(this MauiPicker nativePicker, IPicker picker, ColorStateList? defaultColor)
		{
			var titleColor = picker.TitleColor;

			if (titleColor.IsDefault)
			{
				nativePicker.SetHintTextColor(defaultColor);
			}
			else
			{
				var androidColor = titleColor.ToNative();

				if (!nativePicker.TextColors.IsOneColor(ColorStates, androidColor))
				{
					var acolor = androidColor.ToArgb();
					nativePicker.SetHintTextColor(new ColorStateList(ColorStates, new[] { acolor, acolor }));
				}
			}
		}

		public static void UpdateSelectedIndex(this MauiPicker nativePicker, IPicker picker) =>
			UpdatePicker(nativePicker, picker);

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
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

			if (titleColor == null)
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

			if (picker.SelectedIndex == -1 || picker.SelectedIndex >= picker.GetCount())
				nativePicker.Text = null;
			else
				nativePicker.Text = picker.GetItem(picker.SelectedIndex);
		}
	}
}
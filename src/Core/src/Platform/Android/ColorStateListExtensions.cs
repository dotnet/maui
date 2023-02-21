using Android.Content.Res;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.Platform
{
	internal static class ColorStateListExtensions
	{
		public static ColorStateList CreateDefault(int color) =>
			PlatformInterop.GetDefaultColorStateList(color);

		public static ColorStateList CreateEditText(int enabled, int disabled) =>
			PlatformInterop.GetEditTextColorStateList(enabled, disabled);

		public static ColorStateList CreateCheckBox(int all) =>
			CreateCheckBox(all, all, all, all);

		public static ColorStateList CreateCheckBox(int enabledChecked, int enabledUnchecked, int disabledChecked, int disabledUnchecked) =>
			PlatformInterop.GetCheckBoxColorStateList(enabledChecked, enabledUnchecked, disabledChecked, disabledUnchecked);

		public static ColorStateList CreateSwitch(int all) =>
			CreateSwitch(all, all, all);

		public static ColorStateList CreateSwitch(int disabled, int on, int normal) =>
			PlatformInterop.GetSwitchColorStateList(disabled, on, normal);

		public static ColorStateList CreateButton(int all) =>
			CreateButton(all, all, all, all);

		public static ColorStateList CreateButton(int enabled, int disabled, int off, int pressed) =>
			PlatformInterop.GetButtonColorStateList(enabled, disabled, off, pressed);
	}
}
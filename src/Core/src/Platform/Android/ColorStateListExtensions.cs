using Android.Content.Res;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.Platform
{
	internal static class ColorStateListExtensions
	{
		public static bool IsOneColor(this ColorStateList? csl, int[][] states, AColor color)
		{
			if (csl == null)
				return false;

			if (states.Length == 0)
				return false;

			for (int i = 0; i < states.Length; i++)
			{
				var colorState = states[i];
				if (csl.GetColorForState(colorState, color) != color)
					return false;
			}

			return true;
		}

		public static ColorStateList CreateDefault(int color) =>
			new ColorStateList(ColorStates.Default, new[] { color });

		public static ColorStateList CreateEditText(int all) =>
			CreateEditText(all, all);

		public static ColorStateList CreateEditText(int enabled, int disabled) =>
			new ColorStateList(ColorStates.EditText, new[] { enabled, disabled });

		public static ColorStateList CreateCheckBox(int all) =>
			CreateCheckBox(all, all, all, all);

		public static ColorStateList CreateCheckBox(int enabledChecked, int enabledUnchecked, int disabledChecked, int disabledUnchecked) =>
			new ColorStateList(ColorStates.CheckBox, new[] { enabledChecked, enabledUnchecked, disabledChecked, disabledUnchecked });

		public static ColorStateList CreateSwitch(int all) =>
			CreateSwitch(all, all, all);

		public static ColorStateList CreateSwitch(int disabled, int on, int normal) =>
			new ColorStateList(ColorStates.Switch, new[] { disabled, on, normal });

		public static ColorStateList CreateButton(int all) =>
			CreateButton(all, all, all, all);

		public static ColorStateList CreateButton(int enabled, int disabled, int off, int pressed) =>
			new ColorStateList(ColorStates.Button, new[] { enabled, disabled, off, pressed });
	}
}
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public static class CheckBoxExtensions
	{
		public static void UpdateIsChecked(this CheckBox platformCheckBox, ICheckBox check)
		{
			platformCheckBox.IsChecked = check.IsChecked;
		}

		public static void UpdateForeground(this CheckBox platformCheckBox, ICheckBox check)
		{
			var tintBrush = check.Foreground?.ToPlatform();

			if (tintBrush == null)
			{
				platformCheckBox.Resources.RemoveKeys(TintColorResourceKeys);
				platformCheckBox.Foreground = null;
			}
			else
			{
				platformCheckBox.Resources.SetValueForAllKey(TintColorResourceKeys, tintBrush);
				platformCheckBox.Foreground = tintBrush;
			}

			platformCheckBox.RefreshThemeResources();
		}

		// ResourceKeys controlling the stroke and the checked fill color of the CheckBox.
		// https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.checkbox?view=winui-3.0#control-style-and-template
		static readonly string[] TintColorResourceKeys =
		{
			"CheckBoxCheckBackgroundFillChecked",
			"CheckBoxCheckBackgroundFillCheckedPointerOver",
			"CheckBoxCheckBackgroundFillCheckedPressed",
			"CheckBoxCheckBackgroundFillCheckedDisabled",
			"CheckBoxCheckBackgroundStrokeUnchecked",
			"CheckBoxCheckBackgroundStrokeUncheckedPointerOver",
			"CheckBoxCheckBackgroundStrokeUncheckedPressed",
			"CheckBoxCheckBackgroundStrokeUncheckedDisabled",
			"CheckBoxCheckBackgroundStrokeChecked",
			"CheckBoxCheckBackgroundStrokeCheckedPointerOver",
			"CheckBoxCheckBackgroundStrokeCheckedPressed",
			"CheckBoxCheckBackgroundStrokeCheckedDisabled",
			"CheckBoxCheckBackgroundStrokeIndeterminate",
			"CheckBoxCheckBackgroundStrokeIndeterminatePointerOver",
			"CheckBoxCheckBackgroundStrokeIndeterminatePressed",
			"CheckBoxCheckBackgroundStrokeIndeterminateDisabled",
		};
	}
}
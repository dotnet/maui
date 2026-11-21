using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Platform
{
	public static class CheckBoxExtensions
	{
		public static void UpdateIsChecked(this CheckBox platformCheckBox, ICheckBox check)
		{
			platformCheckBox.IsChecked = check is ICheckBox2 checkBox2 ? checkBox2.IsChecked : check.IsChecked;
		}

		public static void UpdateForeground(this CheckBox platformCheckBox, ICheckBox check)
		{
			var tintBrush = check.Foreground?.ToPlatform();

			if (tintBrush == null)
			{
				platformCheckBox.Resources.RemoveKeys(_tintColorResourceKeys);
				platformCheckBox.Foreground = null;
			}
			else
			{
				platformCheckBox.Resources.SetValueForAllKey(_tintColorResourceKeys, tintBrush);
				platformCheckBox.Foreground = tintBrush;
			}

			platformCheckBox.RefreshThemeResources();
		}

		// ResourceKeys controlling the stroke and the checked/indeterminate fill color of the CheckBox.
		// https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.checkbox?view=winui-3.0#control-style-and-template
		static readonly string[] _tintColorResourceKeys =
		{
			"CheckBoxCheckBackgroundFillChecked",
			"CheckBoxCheckBackgroundFillCheckedPointerOver",
			"CheckBoxCheckBackgroundFillCheckedPressed",
			"CheckBoxCheckBackgroundFillCheckedDisabled",
			"CheckBoxCheckBackgroundFillIndeterminate",
			"CheckBoxCheckBackgroundFillIndeterminatePointerOver",
			"CheckBoxCheckBackgroundFillIndeterminatePressed",
			"CheckBoxCheckBackgroundFillIndeterminateDisabled",
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
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

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
			var resources = platformCheckBox.Resources;

			foreach (string key in _tintColorResourceKeys)
			{
				if (tintBrush == null)
				{
					resources.Remove(key);
				}
				else
				{
					resources[key] = tintBrush;
				}
			}
		}

		// ResourceKeys controlling the stroke and the checked fill color of the CheckBox.
		// https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.controls.checkbox?view=winui-3.0#control-style-and-template
		static readonly string[] _tintColorResourceKeys =
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
using Microsoft.UI.Xaml.Controls;
using XColor = Microsoft.Maui.Color;

namespace Microsoft.Maui
{
	public static class CheckBoxExtensions
	{
		public static void UpdateIsChecked(this CheckBox nativeCheckBox, ICheckBox check)
		{
			nativeCheckBox.IsChecked = check.IsChecked;
		}
	}
}
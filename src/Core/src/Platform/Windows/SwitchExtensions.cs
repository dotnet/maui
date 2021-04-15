using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public static class SwitchExtensions
	{
		public static void UpdateIsToggled(this ToggleSwitch toggleSwitch, ISwitch view)
		{
			toggleSwitch.IsOn = view.IsOn;
		}
	}
}
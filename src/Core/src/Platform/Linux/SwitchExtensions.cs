using Gtk;

namespace Microsoft.Maui
{
	public static class SwitchExtensions
	{
		public static void UpdateIsToggled(this Switch nativeSwitch, ISwitch virtualSwitch)
		{
			nativeSwitch.Active = virtualSwitch.IsToggled;
		}
	}
}
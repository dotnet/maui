using Gtk;

namespace Microsoft.Maui
{
	public static class SwitchExtensions
	{
		public static void UpdateIsOn(this Switch nativeSwitch, ISwitch virtualSwitch)
		{
			nativeSwitch.Active = virtualSwitch.IsOn;
		}
	}
}
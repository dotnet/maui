using Gtk;

namespace Microsoft.Maui
{
	public static class SwitchExtensions
	{
		public static void UpdateIsOn(this Switch platformView, ISwitch virtualSwitch)
		{
			platformView.Active = virtualSwitch.IsOn;
		}
	}
}
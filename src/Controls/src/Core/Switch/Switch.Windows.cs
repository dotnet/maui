using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;

namespace Microsoft.Maui.Controls
{
	public partial class Switch
	{
		public static void MapShowStatusLabel(ISwitchHandler handler, Switch switchControl)
		{
			Platform.SwitchExtensions.UpdateShowStatusLabel(handler.PlatformView, switchControl,
				switchControl.On<PlatformConfiguration.Windows>().GetShowStatusLabel());
		}

		public static void MapShowStatusLabel(SwitchHandler handler, Switch switchControl) =>
			MapShowStatusLabel((ISwitchHandler)handler, switchControl);
	}
}

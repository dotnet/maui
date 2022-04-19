using Tizen.UIExtensions.Common;

namespace Microsoft.Maui.Controls.Platform
{
	public static class ShellExtensions
	{
		public static DrawerBehavior ToPlatform(this FlyoutBehavior behavior)
		{
			if (behavior == FlyoutBehavior.Disabled)
				return DrawerBehavior.Disabled;
			else if (behavior == FlyoutBehavior.Locked)
				return DrawerBehavior.Locked;
			else
				return DrawerBehavior.Drawer;
		}
	}
}
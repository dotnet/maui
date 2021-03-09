using System;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.TV
{
	public class TVShellRenderer : ShellRenderer
	{
		protected override INavigationDrawer CreateNavigationDrawer()
		{
			var drawer = new TVNavigationDrawer(Forms.NativeParent);
			((IShellController)Element).AddFlyoutBehaviorObserver(drawer);
			return drawer;
		}

		protected override ShellItemRenderer CreateShellItemRenderer(ShellItem item)
		{
			return new TVShellItemRenderer(item);
		}

		protected override INavigationView CreateNavigationView()
		{
			return new TVNavigationView(Forms.NativeParent, Element);
		}

		protected override void UpdateFlyoutIsPresented()
		{
			NavigationDrawer.IsOpen = Element.FlyoutIsPresented;
		}
	}
}
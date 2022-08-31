using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Platform
{
	public class MauiTVFlyoutView : TVNavigationDrawer, IToolbarContainer
	{
		void IToolbarContainer.SetToolbar(MauiToolbar toolbar)
		{
			if (Content is IToolbarContainer container)
			{
				container.SetToolbar(toolbar);
			}
		}
	}
}
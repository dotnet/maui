using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Platform
{
	public class MauiFlyoutView : NavigationDrawer, IToolbarContainer
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
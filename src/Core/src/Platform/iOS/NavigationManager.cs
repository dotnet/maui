using UIKit;

namespace Microsoft.Maui.Platform
{
	public class NavigationManager
	{
		IMauiContext MauiContext { get; }

		public IToolbarElement? ToolbarElement { get; private set; }

		public UINavigationController? NavigationController { get; private set; }

		public NavigationManager(IMauiContext mauiContext)
		{
			MauiContext = mauiContext;
		}

		public void SetToolbarElement(IToolbarElement toolbarElement)
		{
			ToolbarElement = toolbarElement;
		}

		public void SetNavigationController(UINavigationController navigationController)
		{
			NavigationController = navigationController;
		}
	}
}

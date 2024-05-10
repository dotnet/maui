using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ToolbarHandler : ElementHandler<IToolbar, UINavigationBar>
	{
		NavigationManager? NavigationManager => MauiContext?.GetNavigationManager();

		public UINavigationController NavigationController => NavigationManager?.NavigationController ?? throw new NullReferenceException("Could not obtain NavigationController.");

		protected override UINavigationBar CreatePlatformElement()
		{
			return NavigationManager?.NavigationController?.NavigationBar ?? throw new NullReferenceException("Could not obtain NavigationBar.");
		}

		public static void MapTitle(IToolbarHandler arg1, IToolbar arg2)
		{
			if (arg1 is ToolbarHandler toolbarHandler)
			{
				toolbarHandler.NavigationController.TopViewController?.UpdateNavigationBarTitle(arg2.Title);
			}
		}

		public static void MapIsVisible(IToolbarHandler handler, IToolbar toolbar)
		{
			if (handler is ToolbarHandler toolbarHandler)
			{
				toolbarHandler.NavigationController.UpdateNavigationBarVisibility(toolbar.IsVisible, true); // TODO: maybe this needs to go through the ViewController (top one?)
			}
		}
	}
}

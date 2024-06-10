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
			return NavigationManager?.NavigationController?.NavigationBar ?? throw new NullReferenceException("Could not obtain Navigation bar.");
		}

		// TODO: Check if this happens for all toolbars, not just NavigationPageToolbar
		public static void MapTitle(IToolbarHandler arg1, IToolbar arg2)
		{
			if (arg1.PlatformView is MauiNavigationBar navigationBar
				&& navigationBar.NavigationController.GetTargetOrDefault() is UINavigationController navigationController)
			{
				navigationController.TopViewController?.UpdateNavigationBarTitle(arg2.Title);
			}
		}

		public static void MapIsVisible(IToolbarHandler handler, IToolbar toolbar)
		{
			if (handler.PlatformView is MauiNavigationBar navigationBar
				&& navigationBar.NavigationController.GetTargetOrDefault() is UINavigationController navigationController)
			{
				navigationController.UpdateNavigationBarVisibility(toolbar.IsVisible, true);
			}
		}

		public static void MapBackButtonVisible(IToolbarHandler handler, IToolbar toolbar)
		{
			if (handler.PlatformView is MauiNavigationBar navigationBar
				&& navigationBar.NavigationController.GetTargetOrDefault() is UINavigationController navigationController)
			{
				navigationController.UpdateBackButtonVisibility(toolbar.BackButtonVisible);
			}
		}
	}
}

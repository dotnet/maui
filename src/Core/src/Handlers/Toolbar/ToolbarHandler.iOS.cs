using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ToolbarHandler : ElementHandler<IToolbar, UINavigationBar>
	{
		NavigationManager? NavigationManager => MauiContext?.GetNavigationManager();

		protected override UINavigationBar CreatePlatformElement()
		{
			return NavigationManager?.NavigationController?.NavigationBar ?? throw new NullReferenceException("Could not obtain NavigationBar.");
		}

		public static void MapTitle(IToolbarHandler arg1, IToolbar arg2)
		{
		}
	}
}

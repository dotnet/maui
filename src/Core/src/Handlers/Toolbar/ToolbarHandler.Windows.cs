using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class ToolbarHandler : ElementHandler<IToolbar, MauiToolbar>
	{
		protected override MauiToolbar CreatePlatformElement()
		{
			return new MauiToolbar();
		}

		public static void MapTitle(IToolbarHandler arg1, IToolbar arg2)
		{
			arg1.PlatformView.UpdateTitle(arg2);
		}

		private protected override void OnDisconnectHandler(object platformView)
		{
			base.OnDisconnectHandler(platformView);
			if (platformView is MauiToolbar mauiToolbar)
			{
				var navRootManager = MauiContext?.GetNavigationRootManager();
				if (navRootManager != null && navRootManager.Toolbar == mauiToolbar)
					navRootManager.SetToolbar(null);
			}
		}
	}
}

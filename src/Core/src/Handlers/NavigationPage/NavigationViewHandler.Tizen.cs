#nullable enable

namespace Microsoft.Maui.Handlers
{
	public partial class NavigationViewHandler :
		ViewHandler<IStackNavigationView, StackNavigationManager>, IPlatformViewHandler
	{
		protected override StackNavigationManager CreatePlatformView()
		{
			return new StackNavigationManager();
		}

		protected override void ConnectHandler(StackNavigationManager platformView)
		{
			base.ConnectHandler(platformView);
			platformView.Connect(VirtualView);
		}

		protected override void DisconnectHandler(StackNavigationManager platformView)
		{
			if (!platformView.HasBody())
				return;

			base.DisconnectHandler(platformView);
			platformView.Disconnect();
		}

		public static void RequestNavigation(INavigationViewHandler arg1, IStackNavigation arg2, object? arg3)
		{
			if (arg1 is NavigationViewHandler platformHandler && arg3 is NavigationRequest ea)
				platformHandler.PlatformView?.RequestNavigation(ea);
		}
	}
}

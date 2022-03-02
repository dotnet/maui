#nullable enable

using System.Collections.Generic;
using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Handlers
{
	// TODO : Need to implement
	public partial class NavigationViewHandler :
		ViewHandler<IStackNavigationView, NavigationStack>, IPlatformViewHandler
	{
		public IStackNavigationView NavigationView => ((IStackNavigationView)VirtualView);

		public IReadOnlyList<IView> NavigationStack { get; private set; } = new List<IView>();

		protected override NavigationStack CreatePlatformView()
		{
			return new NavigationStack()
			{
				PushAnimation = (v, p) => v.Opacity = 0.5f + 0.5f * (float)p,
				PopAnimation = (v, p) => v.Opacity = 0.5f + 0.5f * (float)(1 - p)
			};
		}

		public static void RequestNavigation(INavigationViewHandler arg1, IStackNavigationView arg2, object? arg3)
		{
			if (arg1 is NavigationViewHandler platformHandler && arg3 is NavigationRequest navigationRequest)
			{
				platformHandler.NavigationStack = navigationRequest.NavigationStack;
			}
			//if (arg3 is NavigationRequest args)
			//	arg1.OnPushRequested(args);
		}

		protected override void ConnectHandler(NavigationStack nativeView)
		{
			base.ConnectHandler(nativeView);
			if (VirtualView == null)
				return;
		}

		protected override void DisconnectHandler(NavigationStack nativeView)
		{
			base.DisconnectHandler(nativeView);
		}
	}
}

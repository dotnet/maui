#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIView;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.Maui.Platform.RootNavigationView;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

using System;

namespace Microsoft.Maui.Handlers
{
	public partial class FlyoutViewHandler : IFlyoutViewHandler
	{
		public static IPropertyMapper<IFlyoutView, IFlyoutViewHandler> Mapper = new PropertyMapper<IFlyoutView, IFlyoutViewHandler>(ViewHandler.ViewMapper)
		{
#if ANDROID || WINDOWS
			[nameof(IFlyoutView.Flyout)] = MapFlyout,
			[nameof(IFlyoutView.Detail)] = MapDetail,
			[nameof(IFlyoutView.IsPresented)] = MapIsPresented,
			[nameof(IFlyoutView.FlyoutBehavior)] = MapFlyoutBehavior,
			[nameof(IFlyoutView.FlyoutWidth)] = MapFlyoutWidth,
			[nameof(IFlyoutView.IsGestureEnabled)] = MapIsGestureEnabled,
			[nameof(IToolbarElement.Toolbar)] = MapToolbar,
#endif
		};

		public static CommandMapper<IFlyoutView, IFlyoutViewHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public FlyoutViewHandler() : base(Mapper)
		{
		}

		IFlyoutView IFlyoutViewHandler.VirtualView => VirtualView;

		PlatformView IFlyoutViewHandler.PlatformView => PlatformView;
	}
}

#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIView;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.Maui.Platform.RootNavigationView;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.NUI.DrawerView;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

using System;

namespace Microsoft.Maui.Handlers
{
	public partial class FlyoutViewHandler : IFlyoutViewHandler
	{
		// Like IViewHandler.ContainerView, those properties should be set with priority because other mappers depend on them (like IToolbarElement.Toolbar).
		// So we have a separate mapper for them.
		private static readonly IPropertyMapper<IFlyoutView, IFlyoutViewHandler> FlyoutLayoutMapper = new PropertyMapper<IFlyoutView, IFlyoutViewHandler>()
		{
			[nameof(IFlyoutView.Flyout)] = MapFlyout,
			[nameof(IFlyoutView.Detail)] = MapDetail,
		};

		public static IPropertyMapper<IFlyoutView, IFlyoutViewHandler> Mapper = new PropertyMapper<IFlyoutView, IFlyoutViewHandler>(ViewHandler.ViewMapper, FlyoutLayoutMapper)
		{
			[nameof(IFlyoutView.IsPresented)] = MapIsPresented,
			[nameof(IFlyoutView.FlyoutBehavior)] = MapFlyoutBehavior,
			[nameof(IFlyoutView.FlyoutWidth)] = MapFlyoutWidth,
			[nameof(IFlyoutView.IsGestureEnabled)] = MapIsGestureEnabled,
#if ANDROID || WINDOWS || TIZEN
			[nameof(IToolbarElement.Toolbar)] = MapToolbar,
#endif
		};

		public static CommandMapper<IFlyoutView, IFlyoutViewHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public FlyoutViewHandler() : base(Mapper, CommandMapper)
		{
		}

		public FlyoutViewHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public FlyoutViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IFlyoutView IFlyoutViewHandler.VirtualView => VirtualView;

		PlatformView IFlyoutViewHandler.PlatformView => PlatformView;

#if IOS || MACCATALYST
		/// <summary>
		/// Configuration record filled by Controls layer via RemapForControls().
		/// Core handler calls these when gestures/layout change — Controls writes back to FlyoutPage.
		/// </summary>
		internal sealed record FlyoutViewHandlerControlsConfiguration(
			Action<IFlyoutView, bool> OnPresentedChangedByGesture,
			Action<IFlyoutView, Graphics.Rect, Graphics.Rect> OnLayoutBoundsChanged,
			Action<IFlyoutView> OnLeftBarButtonNeedsUpdate,
			Action<IFlyoutView> OnHandlerDisconnected
		);

		internal static FlyoutViewHandlerControlsConfiguration? ControlsConfiguration { get; set; }
#endif
	}
}

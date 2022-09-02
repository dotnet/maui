using System;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Handlers
{
	public partial class FlyoutViewHandler : ViewHandler<IFlyoutView, DrawerView>
	{
		protected override DrawerView CreatePlatformView()
		{
			return DeviceInfo.IsTV ? new MauiTVFlyoutView() : new MauiFlyoutView();
		}

		protected override void ConnectHandler(DrawerView platformView)
		{
			platformView.Toggled += OnToggled;
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(DrawerView platformView)
		{
			platformView.Toggled -= OnToggled;
			base.DisconnectHandler(platformView);
		}

		public static void MapFlyout(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			handler.PlatformView.UpdateFlyout(flyoutView, handler.MauiContext);

		}

		public static void MapDetail(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			handler.PlatformView.UpdateDetail(flyoutView, handler.MauiContext);
		}


		public static void MapIsPresented(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.PlatformView.UpdateIsPresented(flyoutView);
		}

		public static void MapFlyoutBehavior(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.PlatformView.UpdateFlyoutBehavior(flyoutView);
		}

		public static void MapFlyoutWidth(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.PlatformView.UpdateFlyoutWidth(flyoutView);
		}

		public static void MapIsGestureEnabled(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.PlatformView.UpdateIsGestureEnabled(flyoutView);
		}

		public static void MapToolbar(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			ViewHandler.MapToolbar(handler, flyoutView);

			if (handler.VirtualView is not IToolbarElement toolbarElement)
				return;

			if (toolbarElement.Toolbar?.ToPlatform(handler.MauiContext) is MauiToolbar platformToolbar)
			{
				platformToolbar.IconPressed += (s, e) =>
				{
					if (!toolbarElement.Toolbar.BackButtonVisible && (toolbarElement.Toolbar.IsVisible))
						_ = handler.PlatformView.OpenAsync(true);
				};
			}
		}

		void OnToggled(object? sender, EventArgs e)
		{
			VirtualView.IsPresented = PlatformView.IsOpened;
		}
	}
}

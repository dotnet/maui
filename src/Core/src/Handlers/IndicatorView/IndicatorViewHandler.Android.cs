using AView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, MauiPageControl>
	{
		protected override MauiPageControl CreatePlatformView()
		{
			return new MauiPageControl(Context);
		}

		private protected override void OnConnectHandler(AView platformView)
		{
			base.OnConnectHandler(platformView);

			if (platformView is MauiPageControl pageControl)
				pageControl.SetIndicatorView(VirtualView);
		}

		private protected override void OnDisconnectHandler(AView platformView)
		{
			base.OnDisconnectHandler(platformView);

			if (platformView is MauiPageControl pageControl)
				pageControl.SetIndicatorView(null);
		}

		public static void MapCount(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView.UpdateIndicatorCount();
		}

		public static void MapPosition(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView.UpdatePosition();
		}

		public static void MapHideSingle(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView.UpdateIndicatorCount();
		}

		public static void MapMaximumVisible(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView.UpdateIndicatorCount();
		}

		public static void MapIndicatorSize(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView.ResetIndicators();
		}

		public static void MapIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView.ResetIndicators();
		}
		public static void MapSelectedIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView.ResetIndicators();
		}
		public static void MapIndicatorShape(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView.ResetIndicators();
		}
	}
}

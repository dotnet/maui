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
			PlatformView.SetIndicatorView(VirtualView);
		}

		private protected override void OnDisconnectHandler(AView platformView)
		{
			base.OnDisconnectHandler(platformView);
			PlatformView.SetIndicatorView(null);
		}

		public static void MapCount(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView.UpdateIndicatorCount();
		}

		public static void MapPosition(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView.UpdatePosition();
		}

		public static void MapHideSingle(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView.UpdateIndicatorCount();
		}

		public static void MapMaximumVisible(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView.UpdateIndicatorCount();
		}

		public static void MapIndicatorSize(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView.ResetIndicators();
		}

		public static void MapIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView.ResetIndicators();
		}
		public static void MapSelectedIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView.ResetIndicators();
		}
		public static void MapIndicatorShape(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView.ResetIndicators();
		}
	}
}

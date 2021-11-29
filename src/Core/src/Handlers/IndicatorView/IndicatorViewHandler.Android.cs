using AView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, MauiPageControl>
	{
		protected override MauiPageControl CreateNativeView()
		{
			return new MauiPageControl(Context);
		}

		private protected override void OnConnectHandler(AView nativeView)
		{
			base.OnConnectHandler(nativeView);
			NativeView.SetIndicatorView(VirtualView);
		}

		private protected override void OnDisconnectHandler(AView nativeView)
		{
			base.OnDisconnectHandler(nativeView);
			NativeView.SetIndicatorView(null);
		}

		public static void MapCount(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.NativeView.UpdateIndicatorCount();
		}

		public static void MapPosition(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.NativeView.UpdatePosition();
		}

		public static void MapHideSingle(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.NativeView.UpdateIndicatorCount();
		}

		public static void MapMaximumVisible(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.NativeView.UpdateIndicatorCount();
		}

		public static void MapIndicatorSize(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.NativeView.ResetIndicators();
		}

		public static void MapIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.NativeView.ResetIndicators();
		}
		public static void MapSelectedIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.NativeView.ResetIndicators();
		}
		public static void MapIndicatorShape(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.NativeView.ResetIndicators();
		}
	}
}

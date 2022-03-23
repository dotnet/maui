using AView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, AView>
	{
		MauiPageControl? GetMauiPageControl() => PlatformView as MauiPageControl;
		static MauiPageControl? GetMauiPageControl(IIndicatorViewHandler handler) => handler.PlatformView as MauiPageControl;

		protected override AView CreatePlatformView()
		{
			return new MauiPageControl(Context);
		}

		private protected override void OnConnectHandler(AView platformView)
		{
			base.OnConnectHandler(platformView);
			GetMauiPageControl()?.SetIndicatorView(VirtualView);
		}

		private protected override void OnDisconnectHandler(AView platformView)
		{
			base.OnDisconnectHandler(platformView);
			GetMauiPageControl()?.SetIndicatorView(null);
		}

		public static void MapCount(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			GetMauiPageControl(handler)?.UpdateIndicatorCount();
		}

		public static void MapPosition(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			GetMauiPageControl(handler)?.UpdatePosition();
		}

		public static void MapHideSingle(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			GetMauiPageControl(handler)?.UpdateIndicatorCount();
		}

		public static void MapMaximumVisible(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			GetMauiPageControl(handler)?.UpdateIndicatorCount();
		}

		public static void MapIndicatorSize(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			GetMauiPageControl(handler)?.ResetIndicators();
		}

		public static void MapIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			GetMauiPageControl(handler)?.ResetIndicators();
		}

		public static void MapSelectedIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			GetMauiPageControl(handler)?.ResetIndicators();
		}

		public static void MapIndicatorShape(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			GetMauiPageControl(handler)?.ResetIndicators();
		}
	}
}

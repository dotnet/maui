namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, MauiPageControl>
	{
		protected override MauiPageControl CreatePlatformView() => new MauiPageControl();

		protected override void ConnectHandler(MauiPageControl platformView)
		{
			base.ConnectHandler(platformView);

			platformView?.SetIndicatorView(VirtualView);
		}

		protected override void DisconnectHandler(MauiPageControl platformView)
		{
			base.DisconnectHandler(platformView);

			platformView?.SetIndicatorView(null);
		}

		public static void MapCount(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView?.UpdateIndicatorCount();
		}

		internal static void MapFlowDirection(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			ViewMapper.UpdateProperty(handler, indicator, nameof(IView.FlowDirection));

			var layoutHandler = (indicator as ITemplatedIndicatorView)?.IndicatorsLayoutOverride?.Handler;
			layoutHandler?.UpdateValue(nameof(IView.FlowDirection));
		}

		public static void MapPosition(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView?.UpdatePosition();
		}

		public static void MapHideSingle(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView?.UpdateHideSingle(indicator);
		}

		public static void MapMaximumVisible(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView?.UpdateIndicatorCount();
		}

		public static void MapIndicatorSize(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView?.UpdateIndicatorSize(indicator);
		}

		public static void MapIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView?.UpdatePagesIndicatorTintColor(indicator);
		}

		public static void MapSelectedIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView?.UpdateCurrentPagesIndicatorTintColor(indicator);
		}

		public static void MapIndicatorShape(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView?.UpdateIndicatorShape(indicator);
		}

		internal static void MapIndicatorsLayoutOverride(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView.UpdateIndicatorTemplate(
				(indicator as ITemplatedIndicatorView)?.IndicatorsLayoutOverride, handler.MauiContext);
			handler.UpdateValue(nameof(IIndicatorView.HideSingle));
		}
	}
}
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, UIPageControl>
	{
		protected override UIPageControl CreatePlatformView() => new MauiPageControl();

		protected override void ConnectHandler(UIPageControl platformView)
		{
			base.ConnectHandler(platformView);
			(PlatformView as MauiPageControl)?.SetIndicatorView(VirtualView);
			UpdateIndicator();
		}

		protected override void DisconnectHandler(UIPageControl platformView)
		{
			base.DisconnectHandler(platformView);
			(PlatformView as MauiPageControl)?.SetIndicatorView(null);
		}

		public static void MapCount(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			(handler.PlatformView as MauiPageControl)?.UpdateIndicatorCount();
		}

		public static void MapPosition(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			(handler.PlatformView as MauiPageControl)?.UpdatePosition();
		}

		public static void MapHideSingle(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView?.UpdateHideSingle(indicator);
		}

		public static void MapMaximumVisible(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			(handler.PlatformView as MauiPageControl)?.UpdateIndicatorCount();
		}

		public static void MapIndicatorSize(IIndicatorViewHandler handler, IIndicatorView indicator)
		{
			(handler.PlatformView as MauiPageControl)?.UpdateIndicatorSize(indicator);
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
			(handler.PlatformView as MauiPageControl)?.UpdateIndicatorShape(indicator);
		}

		void UpdateIndicator()
		{
			if (VirtualView is ITemplatedIndicatorView iTemplatedIndicatorView)
			{
				var indicatorsLayoutOverride = iTemplatedIndicatorView.IndicatorsLayoutOverride;
				UIView? handler;
				if (MauiContext != null && indicatorsLayoutOverride != null)
				{
					ClearIndicators();
					handler = indicatorsLayoutOverride.ToPlatform(MauiContext);
					PlatformView.AddSubview(handler);
				}
			}

			void ClearIndicators()
			{
				foreach (var child in PlatformView.Subviews)
					child.RemoveFromSuperview();
			}
		}
	}
}
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, UIPageControl>
	{
		MauiPageControl? UIPager => PlatformView as MauiPageControl;

		protected override UIPageControl CreatePlatformView() => new MauiPageControl();

		protected override void ConnectHandler(UIPageControl platformView)
		{
			base.ConnectHandler(platformView);
			UIPager?.SetIndicatorView(VirtualView);
			UpdateIndicator();
		}

		protected override void DisconnectHandler(UIPageControl platformView)
		{
			base.DisconnectHandler(platformView);
			UIPager?.SetIndicatorView(null);
		}

		public static void MapCount(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.UIPager?.UpdateIndicatorCount();
		}

		public static void MapPosition(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.UIPager?.UpdatePosition();
		}

		public static void MapHideSingle(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView?.UpdateHideSingle(indicator);
		}

		public static void MapMaximumVisible(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.UIPager?.UpdateIndicatorCount();
		}

		public static void MapIndicatorSize(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.UIPager?.UpdateIndicatorSize(indicator);
		}

		public static void MapIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView?.UpdatePagesIndicatorTintColor(indicator);
		}

		public static void MapSelectedIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.PlatformView?.UpdateCurrentPagesIndicatorTintColor(indicator);
		}

		public static void MapIndicatorShape(IndicatorViewHandler handler, IIndicatorView indicator)
		{
			handler.UIPager?.UpdateIndicatorShape(indicator);
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
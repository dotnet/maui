using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, MauiPageControl>
	{
		protected override MauiPageControl CreatePlatformView() => new MauiPageControl();

		protected override void ConnectHandler(MauiPageControl platformView)
		{
			base.ConnectHandler(platformView);

			platformView?.SetIndicatorView(VirtualView);

			UpdateIndicator();
		}

		public static void MapCount(IIndicatorViewHandler handler, IIndicatorView indicator) => handler.PlatformView?.CreateIndicators();
		public static void MapPosition(IIndicatorViewHandler handler, IIndicatorView indicator) => handler.PlatformView?.UpdateIndicatorsColor();
		public static void MapHideSingle(IIndicatorViewHandler handler, IIndicatorView indicator) => handler.PlatformView?.CreateIndicators();
		public static void MapMaximumVisible(IIndicatorViewHandler handler, IIndicatorView indicator) => handler.PlatformView?.CreateIndicators();
		public static void MapIndicatorSize(IIndicatorViewHandler handler, IIndicatorView indicator) => handler.PlatformView?.CreateIndicators();
		public static void MapIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicator) => handler.PlatformView?.UpdateIndicatorsColor();
		public static void MapSelectedIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicator) => handler.PlatformView?.UpdateIndicatorsColor();
		public static void MapIndicatorShape(IIndicatorViewHandler handler, IIndicatorView indicator) => handler.PlatformView?.CreateIndicators();

		void UpdateIndicator()
		{
			if (VirtualView is ITemplatedIndicatorView iTemplatedIndicatorView)
			{
				var indicatorsLayoutOverride = iTemplatedIndicatorView.IndicatorsLayoutOverride;
				FrameworkElement handler;
				if (MauiContext != null && indicatorsLayoutOverride != null)
				{
					ClearIndicators();
					handler = indicatorsLayoutOverride.ToPlatform(MauiContext);
					if (handler != null)
						PlatformView.ItemsSource = new ObservableCollection<FrameworkElement>() { handler };
				}
			}

			void ClearIndicators()
			{
				PlatformView.Items.Clear();
			}
		}
	}
}

using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, ItemsControl>
	{
		MauiPageControl? MauiPagerControl => NativeView as MauiPageControl;

		protected override ItemsControl CreateNativeView() => new MauiPageControl();

		protected override void ConnectHandler(ItemsControl nativeView)
		{
			base.ConnectHandler(nativeView);
			MauiPagerControl?.SetIndicatorView(VirtualView);
			UpdateIndicator();
		}

		public static void MapCount(IndicatorViewHandler handler, IIndicatorView indicator) => handler.MauiPagerControl?.CreateIndicators();
		public static void MapPosition(IndicatorViewHandler handler, IIndicatorView indicator) => handler.MauiPagerControl?.UpdateIndicatorsColor();
		public static void MapHideSingle(IndicatorViewHandler handler, IIndicatorView indicator) => handler.MauiPagerControl?.CreateIndicators();
		public static void MapMaximumVisible(IndicatorViewHandler handler, IIndicatorView indicator) => handler.MauiPagerControl?.CreateIndicators();
		public static void MapIndicatorSize(IndicatorViewHandler handler, IIndicatorView indicator) => handler.MauiPagerControl?.CreateIndicators();
		public static void MapIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicator) => handler.MauiPagerControl?.UpdateIndicatorsColor();
		public static void MapSelectedIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicator) => handler.MauiPagerControl?.UpdateIndicatorsColor();
		public static void MapIndicatorShape(IndicatorViewHandler handler, IIndicatorView indicator) => handler.MauiPagerControl?.CreateIndicators();

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
						NativeView.ItemsSource = new ObservableCollection<FrameworkElement>() { handler };
				}
			}

			void ClearIndicators()
			{
				NativeView.Items.Clear();
			}
		}
	}
}

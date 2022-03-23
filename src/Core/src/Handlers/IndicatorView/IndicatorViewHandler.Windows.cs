using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, FrameworkElement>
	{
		MauiPageControl? GetMauiPageControl() => PlatformView as MauiPageControl;
		static MauiPageControl? GetMauiPageControl(IIndicatorViewHandler handler) => handler.PlatformView as MauiPageControl;

		protected override FrameworkElement CreatePlatformView() => new MauiPageControl();

		protected override void ConnectHandler(FrameworkElement platformView)
		{
			base.ConnectHandler(platformView);
			GetMauiPageControl()?.SetIndicatorView(VirtualView);
			UpdateIndicator();
		}

		public static void MapCount(IIndicatorViewHandler handler, IIndicatorView indicator) => GetMauiPageControl(handler)?.CreateIndicators();
		public static void MapPosition(IIndicatorViewHandler handler, IIndicatorView indicator) => GetMauiPageControl(handler)?.UpdateIndicatorsColor();
		public static void MapHideSingle(IIndicatorViewHandler handler, IIndicatorView indicator) => GetMauiPageControl(handler)?.CreateIndicators();
		public static void MapMaximumVisible(IIndicatorViewHandler handler, IIndicatorView indicator) => GetMauiPageControl(handler)?.CreateIndicators();
		public static void MapIndicatorSize(IIndicatorViewHandler handler, IIndicatorView indicator) => GetMauiPageControl(handler)?.CreateIndicators();
		public static void MapIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicator) => GetMauiPageControl(handler)?.UpdateIndicatorsColor();
		public static void MapSelectedIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicator) => GetMauiPageControl(handler)?.UpdateIndicatorsColor();
		public static void MapIndicatorShape(IIndicatorViewHandler handler, IIndicatorView indicator) => GetMauiPageControl(handler)?.CreateIndicators();

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
					var pager = GetMauiPageControl();
					if (handler != null && pager != null)
						pager.ItemsSource = new ObservableCollection<FrameworkElement>() { handler };
				}
			}

			void ClearIndicators()
			{
				GetMauiPageControl()?.Items.Clear();
			}
		}
	}
}
